using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Hosting.Internal;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Security.Claims;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Drawing;
using QuestPDF.Elements.Table;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using ZXing.QrCode.Internal;
using Microsoft.AspNetCore.Http.HttpResults;
using static QuestPDF.Helpers.Colors;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations;
using SeniorCitizen.Server.DTOs;
using SeniorCitizen.Server.Models;



namespace OnlineRegistration.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeesDbContext _db;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailQueue _emailQueue;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPgpService _pgpService;

        public EmployeeController(EmployeesDbContext db, IConfiguration config, IEmailQueue emailQueue, IFileService fileService, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor, AppDbContext context, IPgpService pgpService)
        {
            _db = db;
            _config = config;
            _emailQueue = emailQueue;
            _fileService = fileService;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _pgpService = pgpService;
        }


        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- Lookup Tables ----------------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Provides standardized lookup data used across registration forms and biometric profiles.
        /// </summary>
        [HttpGet("gender")]
        public async Task<ActionResult<IEnumerable<Gender>>> GetGenderList()
        {
            var genders = await _db.Gender.AsNoTracking().ToListAsync();
            return Ok(genders);
        }

        /// <summary>
        /// Retrieves all valid Civil Status options (e.g., Single, Married, Widowed).
        /// </summary>
        [HttpGet("civilstatus")]
        public async Task<ActionResult<IEnumerable<CivilStatus>>> GetCivilStatusList()
        {
            var statuses = await _db.CivilStatus.AsNoTracking().ToListAsync();
            return Ok(statuses);
        }

        /// <summary>
        /// Retrieves the list of organizational departments for employee categorization.
        /// </summary>
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartmentList()
        {
            var dts = await _db.Department.AsNoTracking().ToListAsync();
            return Ok(dts);
        }

        /// <summary>
        /// Retrieves standard blood types (A, B, AB, O) and their Rh factors.
        /// </summary>
        [HttpGet("bloodtype")]
        public async Task<ActionResult<IEnumerable<Department>>> GetBloodTypeList()
        {
            var dts = await _db.BloodType.AsNoTracking().ToListAsync();
            return Ok(dts);
        }

        /// <summary>
        /// Retrieves academic classifications (e.g., High School, Vocational, Bachelor's, Post-Grad).
        /// </summary>
        [HttpGet("educationlevel")]
        public async Task<ActionResult<IEnumerable<Department>>> GetEducationLevelList()
        {
            var dts = await _db.EducationLevels.AsNoTracking().ToListAsync();
            return Ok(dts);
        }

        // ---------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------



        /// <summary>
        /// AUTHORIZATION POLICY REFERENCE
        /// -----------------------------
        /// This table documents how the numeric UserRole claim values map to defined authorization policies.
        /// </summary>
        /// 
        /*
        | RoleID (Claim Value) | RoleDesc     |                         Policies Granted Access To (Direct/Inherited)                        |
        |:---------------------|:-------------|:---------------------------------------------------------------------------------------------|
        | 1                    | Super Admin  | RequireSuperAdmin, RequireManagementAccess, RequireAdministrativeView, RequireEmployeeAccess |
        | 2                    | System User  | RequireManagementAccess, RequireAdministrativeView, RequireEmployeeAccess                    |
        | 3                    | Kit Operator | (None defined)                                                                               |
        | 4                    | Employee     | RequireEmployeeAccess (If Role 4 is the Employee role)                                       |
        | 5                    | Citizen      | RequireCitizenAccess (If Role 5 is the Citizen role)                                         |
        | 6                    | HR           | RequireAdministrativeView                                                                    |
        */

        // Note: Management/Admin roles (1 & 2) typically inherit access to lower-level policies like EmployeeAccess.
        // Please ensure the code reflects the intended access for Roles 4 and 5 based on your last configuration.



        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- FILES UPLOAD -----------------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        private const string TempUploadFolder = "temp_uploads";

        /// <summary>
        /// Uploads a file to a temporary directory and returns a unique GUID-based key.
        /// </summary>
        [HttpPost("uploadfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(100_000_000)] // Limit file size
        public async Task<IActionResult> UploadPdsFile(IFormFile file)
        {
            // 1. Validation Checks
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was selected for upload." });
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB limit for documents
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds the limit of 10MB." });
            }

            // Check for allowed extensions (customize as needed for security)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = $"File type {fileExtension ?? "unknown"} is not allowed. Only JPG, PNG, and PDF." });
            }

            try
            {
                // 2. Define the temporary storage path
                var fileKey = Guid.NewGuid().ToString("N") + fileExtension;
                // Ensure the fileKey is entirely lowercase for consistent lookup in FinalizeUpload
                fileKey = fileKey.ToLowerInvariant();
                var originalFileName = Path.GetFileName(file.FileName);


                // Define the temporary path (e.g., wwwroot/temp_uploads)
                var tempPath = Path.Combine(_hostingEnvironment.WebRootPath, TempUploadFolder);

                // Ensure the directory exists
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                // Use 'filePath' as the final path variable.
                var filePath = Path.Combine(tempPath, fileKey);

                // 3. ⭐ CRITICAL STEP: Save the file to the temporary file system ⭐
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 4. ⭐ CRITICAL STEP: Return the unique key/name ⭐
                return Ok(new
                {
                    message = "File uploaded.",
                    fileKey = fileKey,
                    originalFileName = originalFileName

                });
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { message = $"Temporary file upload failed." });
            }
        }

        /// <summary>
        /// Deletes a physical file from the temporary storage folder.
        /// </summary>
        [HttpDelete("deleteupload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeletePdsFile([FromQuery] string fileKey)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(fileKey))
            {
                return BadRequest(new { message = "File is required for deletion." });
            }

            // Define the temporary storage path and target file path
            var tempPath = Path.Combine(_hostingEnvironment.WebRootPath, TempUploadFolder);
            var filePath = Path.Combine(tempPath, fileKey);
            //var originalFileName = Path.GetFileName(fileKey.FileName);

            try
            {
                // 2. Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    // The file might have already been processed/moved or doesn't exist.
                    // Log this as a warning, but return OK/Accepted if the file is not there,
                    // as the client's desired state (file deleted) is achieved.
                    // For security, it's safer to return NotFound for clarity.
                    return NotFound(new { message = "File not found." });
                }

                // 3. Delete the file
                System.IO.File.Delete(filePath);

                // 4. Return success response
                return Ok(new
                {
                    message = "File deleted successfully."
                });
            }
            catch (Exception ex)
            {
                // Log the exception details here
                // E.g., failed to access the file due to permissions
                return StatusCode(500, new { message = "Error deleting file." });
            }
        }

        /// <summary>
        /// Removes the database reference (path and original name) for a specific document type.
        /// </summary>
        [HttpPost("cleardocumentkey")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearDocumentKey([FromQuery] string personId, [FromQuery] string docType)
        {
            if (string.IsNullOrWhiteSpace(personId) || string.IsNullOrWhiteSpace(docType))
            {
                return BadRequest(new { message = "Person ID and Document Type are required." });
            }

            // 1. Validate and Parse Person ID
            if (!int.TryParse(personId, out int id))
            {
                return BadRequest(new { message = "Invalid Person ID format. Must be an integer." });
            }

            try
            {
                // 2. Retrieve the Person entity from the database
                // Assuming your table is mapped to a model named 'Person'
                var person = await _db.PersonalInformations
                 .Include(p => p.Declaration) // <-- This loads the Declaration entity
                 .FirstOrDefaultAsync(p => p.PersonID == id);

                if (person == null)
                {
                    return NotFound(new { message = $"Person with ID: {id} not found." });
                }

                string fieldName;

                // 3. Use a switch statement to update the correct field to NULL
                switch (docType.ToLowerInvariant())
                {
                    case "birthcertificate":
                        person.BirthCertificatePath = null;
                        // ⭐ CRITICAL FIX: Clear the Original Name field as well ⭐
                        person.BirthCertificateOriginalName = null;
                        fieldName = "BirthCertificatePath and BirthCertificateOriginalName";
                        break;

                    case "marriagecertificate":
                        person.MarriageCertificatePath = null;
                        // ⭐ CRITICAL FIX: Clear the Original Name field as well ⭐
                        person.MarriageCertificateOriginalName = null;
                        fieldName = "MarriageCertificatePath and MarriageCertificateOriginalName";
                        break;
                    case "passportphoto": // <--- THIS CASE WAS MISSING/SKIPPED
                        if (person.Declaration == null)
                        {
                            return NotFound(new { message = $"Declaration record for Person ID: {id} not found." });
                        }

                        // ⭐ CRITICAL: Clear the PhotoPath (file key) and PhotoOriginalName
                        person.Declaration.PhotoPath = null;
                        person.Declaration.PhotoOriginalName = null;
                        fieldName = "Declaration.PhotoPath and Declaration.PhotoOriginalName";
                        break;


                    default:
                        return BadRequest(new { message = $"Invalid document type: {docType}. Must be 'BirthCertificate' or 'MarriageCertificate'." });
                }

                // 4. Save the changes back to the database
                await _db.SaveChangesAsync();

                return Ok(new { message = $"{docType} reference cleared successfully from database. Field {fieldName} set to NULL." });
            }
            catch (Exception ex)
            {
                // Log the exception details (e.g., using a logging framework)
                // Console.WriteLine($"Error clearing document key: {ex}");
                return StatusCode(500, new { message = $"Error clearing document key: {ex.Message}" });
            }
        }

        /// <summary>
        /// Serves a file for inline browser preview. Searches temporary storage first, then permanent archives.
        /// </summary>
        [HttpGet("previewupload")]
        public IActionResult PreviewPdsFile([FromQuery] string fileKey)
        {
            if (string.IsNullOrWhiteSpace(fileKey)) return BadRequest("File key is required.");

            // 1. Clean the filename
            string fileName = Path.GetFileName(fileKey);

            // 2. Define possible locations
            string tempPath = Path.Combine(_hostingEnvironment.WebRootPath, TempUploadFolder, fileName);
            string permanentBase = Path.Combine(_hostingEnvironment.WebRootPath, "PermanentPDSFiles");

            string finalPath = "";

            // 3. Logic: Check Temp first, then Search Permanent folders
            if (System.IO.File.Exists(tempPath))
            {
                finalPath = tempPath;
            }
            else
            {
                // Search recursively in PermanentPDSFiles (the "Smart Search" from before)
                var foundFiles = Directory.GetFiles(permanentBase, fileName, SearchOption.AllDirectories);
                if (foundFiles.Any()) finalPath = foundFiles[0];
            }

            if (string.IsNullOrEmpty(finalPath))
            {
                return NotFound(new { message = "File not found in Temp or Permanent storage.", key = fileName });
            }

            // 4. Return as Inline Preview
            var extension = Path.GetExtension(finalPath).ToLowerInvariant();
            string contentType = extension == ".pdf" ? "application/pdf" : "image/jpeg";

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");

            return File(new FileStream(finalPath, FileMode.Open, FileAccess.Read, FileShare.Read), contentType);
        }

        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- EMPLOYEE DASHBOARD -----------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Retrieves the complete Personal Data Sheet (PDS) for a specific citizen.
        /// </summary>
        /// <param name="id">The unique PersonID of the record to retrieve.</param>
        /// <returns>A comprehensive <see cref="PersonalInformationDto"/> containing all 4 sections of the PDS.</returns>
        /// <remarks>
        /// <b>Technical Details:</b>
        /// 1. Uses <c>.Include()</c> for 13 related tables to perform a deep-fetch.
        /// 2. Implements internal <c>LogPdsAction</c> for high-resolution audit trails.
        /// 3. Maps raw database models to a clean DTO structure for frontend consumption.
        /// 4. Utilizes <c>AsNoTracking()</c> to maximize read performance for this large object graph.
        /// </remarks>
        [HttpGet("user/{id}")]
        public async Task<ActionResult<PersonalInformationDto>> GetC1(int id)
        {
            int? userId = null;
            string username = User.FindFirstValue("username"); // Get authenticated username or default

            LogPdsAction("PDS_RETRIEVAL_ATTEMPT", $"Attempting to retrieve PDS for PersonID: {id}.");

            // Attempt to extract the authenticated user's ID
            if (User.Identity.IsAuthenticated && int.TryParse(User.FindFirstValue("id"), out int authenticatedId))
            {
                userId = authenticatedId;
            }

            void LogPdsAction(string description, string specificNote = null)
            {
                var logEntry = new ApplicationLog
                {
                    LogId = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    LogDescription = description,
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LogTime = DateTime.Now.ToString("HH:mm:ss"),
                    Notes = $"PDS ID: {id}. User: {username}. {specificNote}"
                };
                // Use _context since that was the pattern in your AuthController log helper
                _db.ApplicationLogs.Add(logEntry);
            }
            var person = await _db.PersonalInformations
                .Include(p => p.Children)
                .Include(p => p.EducationRecords)
                .Include(p => p.VoluntaryWorks)
                .Include(p => p.Trainings)
                .Include(p => p.SkillsHobbies)
                .Include(p => p.Distinctions)
                .Include(p => p.Memberships)
                .Include(p => p.CivilServiceEligibilities)
                .Include(p => p.WorkExperiences)
                .Include(p => p.PdsSectionIV)
                .Include(p => p.PersonalInformationReferences)
                .Include(p => p.Declaration)

                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonID == id);

            if (person == null)
            {
                LogPdsAction("PDS_RETRIEVAL_FAILURE", $"PDS with PersonID: {id} not found.");
                await _db.SaveChangesAsync();
                return NotFound();
            }

            // Map to DTO
            var dto = new PersonalInformationDto
            {
                PersonID = person.PersonID,
                CsIdNo = person.CsIdNo,
                Surname = person.Surname,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                NameExtension = person.NameExtension,

                DateOfBirth = person.DateOfBirth,
                PlaceOfBirth = person.PlaceOfBirth,

                SexID = person.SexID,
                CivilStatusID = person.CivilStatusID,

                BirthCertificateFileKey = person.BirthCertificatePath != null
                    ? Url.Content(person.BirthCertificatePath)
                    : null,
                BirthCertificateOriginalName = person.BirthCertificateOriginalName,

                MarriageCertificateFileKey = person.MarriageCertificatePath != null
                    ? Url.Content(person.MarriageCertificatePath)
                    : null,
                MarriageCertificateOriginalName = person.MarriageCertificateOriginalName,

                Citizenship = person.Citizenship,

                HeightCM = person.HeightCM,
                WeightKG = person.WeightKG,
                BloodType = person.BloodType,

                GsisID = person.GsisID,
                PagibigID = person.PagibigID,
                PhilhealthID = person.PhilhealthID,
                SssNo = person.SssNo,
                Tin = person.Tin,
                AgencyEmployeeNo = person.AgencyEmployeeNo,

                ResHouseBlockLot = person.ResHouseBlockLot,
                ResBarangay = person.ResBarangay,
                ResCityMunicipality = person.ResCityMunicipality,
                ResSubdivisionVillage = person.ResSubdivisionVillage,
                ResProvince = person.ResProvince,
                ResZip = person.ResZip,

                PermHouseBlockLot = person.PermHouseBlockLot,
                PermBarangay = person.PermBarangay,
                PermCityMunicipality = person.PermCityMunicipality,
                PermSubdivisionVillage = person.PermSubdivisionVillage,
                PermProvince = person.PermProvince,
                PermZip = person.PermZip,

                TelephoneNo = person.TelephoneNo,
                MobileNo = person.MobileNo,
                Email = person.Email,

                SpouseSurname = person.SpouseSurname,
                SpouseFirstName = person.SpouseFirstName,
                SpouseMiddleName = person.SpouseMiddleName,
                SpouseNameExtension = person.SpouseNameExtension,
                SpouseOccupation = person.SpouseOccupation,
                SpouseEmployer = person.SpouseEmployer,
                SpouseBusinessAddress = person.SpouseBusinessAddress,
                SpouseTelephone = person.SpouseTelephone,

                FatherSurname = person.FatherSurname,
                FatherFirstName = person.FatherFirstName,
                FatherMiddleName = person.FatherMiddleName,
                FatherNameExtension = person.FatherNameExtension,

                MotherSurname = person.MotherSurname,
                MotherFirstName = person.MotherFirstName,
                MotherMiddleName = person.MotherMiddleName,
                MotherNameExtension = person.MotherNameExtension,

                DepartmentID = person.DepartmentID,
                Designation = person.Designation,

                Children = person.Children.Select(c => new ChildDto
                {
                    ChildID = c.ChildId,
                    PersonID = c.PersonID,
                    FullName = c.FullName,
                    DateOfBirth = c.DateOfBirth
                }).ToList(),

                EducationRecords = person.EducationRecords.OrderBy(e => e.OrderIndex).Select(e => new EducationDto
                {
                    Level = e.Level,
                    SchoolName = e.SchoolName,
                    Degree = e.Degree,
                    AttendanceFrom = e.AttendanceFrom,
                    AttendanceTo = e.AttendanceTo,
                    HighestLevel = e.HighestLevel,
                    YearGraduated = e.YearGraduated,
                    Honors = e.Honors,
                    OrderIndex = e.OrderIndex
                }).ToList(),

                VoluntaryWorks = person.VoluntaryWorks.Select(v => new VoluntaryWorkDTO
                {
                    WorkId = v.WorkId,
                    PersonID = v.PersonID,
                    Organization = v.Organization,
                    DateFrom = v.DateFrom,
                    DateTo = v.DateTo,
                    NumberOfHours = v.NumberOfHours,
                    Position = v.Position
                }).ToList(),

                Trainings = person.Trainings.Select(t => new TrainingDTO
                {
                    TrainingId = t.TrainingId,
                    PersonID = t.PersonID,
                    Title = t.Title,
                    DateFrom = t.DateFrom,
                    DateTo = t.DateTo,
                    NumberOfHours = t.NumberOfHours,
                    TypeOfLD = t.TypeOfLD,
                    ConductedBy = t.ConductedBy
                }).ToList(),

                SkillsHobbies = person.SkillsHobbies.Select(s => new SkillHobbyDTO
                {
                    SkillId = s.SkillId,
                    PersonID = s.PersonID,
                    SkillOrHobby = s.SkillOrHobby
                }).ToList(),

                Distinctions = person.Distinctions.Select(d => new DistinctionDTO
                {
                    DistinctionId = d.DistinctionId,
                    PersonID = d.PersonID,
                    Distinction = d.DistinctionName
                }).ToList(),

                Memberships = person.Memberships.Select(m => new MembershipDTO
                {
                    MembershipId = m.MembershipId,
                    PersonID = m.PersonID,
                    OrganizationName = m.OrganizationName
                }).ToList(),

                CivilServiceEligibilities = person.CivilServiceEligibilities.Select(e => new CivilServiceEligibilityDTO
                {
                    EligibilityID = e.EligibilityID,
                    PersonID = e.PersonID,
                    CareerService = e.CareerService,
                    Rating = e.Rating,
                    DateOfExamination = e.DateOfExamination,
                    PlaceOfExamination = e.PlaceOfExamination,
                    LicenseNumber = e.LicenseNumber,
                    LicenseValidity = e.LicenseValidity,
                    DateReleased = e.DateReleased
                }).ToList(),

                WorkExperiences = person.WorkExperiences.Select(w => new WorkExperienceDTO
                {
                    WorkExperienceID = w.WorkExperienceId,
                    PersonID = w.PersonID,
                    DateFrom = w.DateFrom,
                    DateTo = w.DateTo,
                    PositionTitle = w.PositionTitle,
                    DepartmentAgencyCompany = w.DepartmentAgencyCompany,
                    MonthlySalary = w.MonthlySalary,
                    SalaryGradeStep = w.SalaryGradeStep,
                    StatusOfAppointment = w.StatusOfAppointment,
                    IsGovernmentService = w.IsGovernmentService,
                    Description = w.Description
                }).ToList(),

                PdsSectionIV = person.PdsSectionIV != null ? new PdsSectionIVDto
                {
                    PdsSectionIVId = person.PdsSectionIV.PdsSectionIVId,

                    // --- 34. Relatives (Model -> DTO) ---
                    Q34a_RelatedWithin3rd = person.PdsSectionIV.Q34a_RelatedWithin3rd,
                    Q34b_RelatedWithin4th = person.PdsSectionIV.Q34b_RelatedWithin4th,
                    Q34_Details = person.PdsSectionIV.Q34_Details,

                    // --- 35. Administrative/Criminal Charges ---
                    Q35a_AdminOffense = person.PdsSectionIV.Q35a_AdminOffense,
                    Q35b_CriminallyCharged = person.PdsSectionIV.Q35b_CriminallyCharged,

                    Q35a_AdminDetails = person.PdsSectionIV.Q35a_Details,
                    Q35b_CriminalDetails = person.PdsSectionIV.Q35b_Details,

                    Q35b_DateFiled = person.PdsSectionIV.Q35b_DateFiled,
                    Q35b_Status = person.PdsSectionIV.Q35b_Status,

                    // --- 36. Convicted of Crime ---
                    Q36_Convicted = person.PdsSectionIV.Q36_Convicted,
                    Q36_Details = person.PdsSectionIV.Q36_Details,

                    // --- 37. Separation from Service ---
                    Q37_Separated = person.PdsSectionIV.Q37_Separated,
                    Q37_Details = person.PdsSectionIV.Q37_Details,

                    // --- 38. Resigned for Political Campaign ---
                    Q38a_Candidate = person.PdsSectionIV.Q38a_Candidate,
                    Q38b_ResignedForCampaign = person.PdsSectionIV.Q38b_ResignedForCampaign,
                    Q38a_Details = person.PdsSectionIV.Q38a_Details,
                    Q38b_Details = person.PdsSectionIV.Q38b_Details,

                    // --- 39. Immigrant/Permanent Resident Status & Dual Citizenship ---
                    Q39_Details_Country = person.PdsSectionIV.Q39_Details_Country,
                    Q39_Country = person.PdsSectionIV.Q39_Country,

                    // --- 40. Other Information ---
                    Q40a_IndigenousGroup = person.PdsSectionIV.Q40a_IndigenousGroup,
                    Q40a_Details = person.PdsSectionIV.Q40a_Details,
                    Q40b_Disability = person.PdsSectionIV.Q40b_Disability,
                    Q40b_Details_IDNo = person.PdsSectionIV.Q40b_Details_IDNo,
                    Q40c_SoloParent = person.PdsSectionIV.Q40c_SoloParent,
                    Q40c_Details_IDNo = person.PdsSectionIV.Q40c_Details_IDNo
                } : null,

                References = person.PersonalInformationReferences.Select(r => new ReferenceDto
                {
                    ReferenceID = r.ReferenceID,
                    Name = r.Name,
                    OfficeOrResidentialAddress = r.OfficeOrResidentialAddress,
                    ContactNoOrEmail = r.ContactNoOrEmail
                }).ToList(),

                Declaration = person.Declaration != null ? new PdsSectionVDeclarationDto
                {
                    DeclarationID = person.Declaration.DeclarationID,
                    GovernmentIssuedID = person.Declaration.GovernmentIssuedID,
                    LicensePassportIDNo = person.Declaration.LicensePassportIDNo,
                    DateOfIssuance = person.Declaration.DateOfIssuance,
                    PlaceOfIssuance = person.Declaration.PlaceOfIssuance,
                    DateAccomplished = person.Declaration.DateAccomplished,

                    Signature = person.Declaration.SignaturePath,
                    AdminSignature = person.Declaration.AdminSignature,

                    PhotoFileKey = person.Declaration.PhotoPath,
                    PhotoOriginalName = person.Declaration.PhotoOriginalName,

                    RightThumbmarkFileKey = person.Declaration.RightThumbmarkPath,
                    DateSchedule = person.Declaration.DateSchedule
                } : null,
            };

            LogPdsAction("PDS_RETRIEVAL_SUCCESS", $"Successfully retrieved PDS for PersonID: {id}.");
            await _db.SaveChangesAsync();
            return Ok(dto);
        }

        /// <summary>
        /// Creates or Updates (Upserts) a complete Personal Data Sheet record.
        /// </summary>
        /// <param name="id">The PersonID to target.</param>
        /// <param name="dto">The full PDS dataset from the frontend.</param>
        /// <remarks>
        /// <b>Logic Flow:</b>
        /// 1. <b>Audit:</b> Logs the attempt.
        /// 2. <b>Load:</b> Fetches the existing graph (13+ tables).
        /// 3. <b>Validate:</b> Checks required fields, email formats, and year ranges for Education/Work/Training.
        /// 4. <b>File Finalization:</b> Moves temporary uploads (GUIDs) to permanent storage via <c>_fileService</c>.
        /// 5. <b>Sync:</b> Automatically updates the <c>EmployeeIDApplication</c> record to match PDS data.
        /// </remarks>
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpsertC1(int id, PersonalInformationDto dto)
        {
            int? userId = null;
            string username = User.FindFirstValue("username"); // Get authenticated username or default

            // Attempt to extract the authenticated user's ID
            if (User.Identity.IsAuthenticated && int.TryParse(User.FindFirstValue("id"), out int authenticatedId))
            {
                userId = authenticatedId;
            }

            LogPdsAction("PDS_UPDATE_ATTEMPT", "Received request to upsert Personal Data Sheet.");

            // 💡 Local function for logging PDS actions (using your provided pattern)
            void LogPdsAction(string description, string specificNote = null)
            {
                var logEntry = new ApplicationLog
                {
                    LogId = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    LogDescription = description,
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LogTime = DateTime.Now.ToString("HH:mm:ss"),
                    Notes = $"PDS ID: {id}. User: {username}. {specificNote}"
                };
                // Use _context since that was the pattern in your AuthController log helper
                _db.ApplicationLogs.Add(logEntry);
            }

            var person = await _db.PersonalInformations
                .Include(p => p.Children)
                .Include(p => p.EducationRecords)
                .Include(p => p.VoluntaryWorks)
                .Include(p => p.Trainings)
                .Include(p => p.SkillsHobbies)
                .Include(p => p.Distinctions)
                .Include(p => p.Memberships)
                .Include(p => p.CivilServiceEligibilities)
                .Include(p => p.WorkExperiences)
                .Include(p => p.PdsSectionIV)
                .Include(p => p.PersonalInformationReferences)
                .Include(p => p.Declaration)
                .FirstOrDefaultAsync(p => p.PersonID == id);

            if (person == null)
            {
                person = new PersonalInformation { PersonID = id }; // Ensure ID is set here
                _db.PersonalInformations.Add(person);
                LogPdsAction("PDS_RECORD_INSERT_INIT", "New record.");
            }
            else
            {
                // The record exists and is being tracked. 
                // EF will automatically detect changes to its properties.
                LogPdsAction("PDS_RECORD_UPDATE_INIT", "Existing record.");

                // Explicitly tell EF this is a modified record if you aren't using Auto-Detect Changes
                //_db.Entry(person).State = EntityState.Modified;
            }

            // 2. Basic Required Fields Check
            if (string.IsNullOrWhiteSpace(dto.Surname) || string.IsNullOrWhiteSpace(dto.FirstName))
            {
                LogPdsAction("PDS_VALIDATION_FAILURE_REQUIRED_FIELD", "Surname or First Name missing.");
                return BadRequest(new { message = "Surname and First Name are required fields." });
            }

            // 5. Email Format Validation (Helps prevent CHECK constraint errors)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
            {
                LogPdsAction("PDS_VALIDATION_FAILURE_EMAIL_FORMAT", $"Email '{dto.Email}' failed format check.");
                return BadRequest(new { message = "The provided Email address is not in a valid format." });
            }

            if (dto.EducationRecords != null)
            {
                foreach (var eduDto in dto.EducationRecords)
                {
                    // Check if both AttendanceFrom (Year) and AttendanceTo (Year) have values
                    if (eduDto.AttendanceFrom.HasValue && eduDto.AttendanceTo.HasValue)
                    {
                        int startYear = eduDto.AttendanceFrom.Value;
                        int endYear = eduDto.AttendanceTo.Value;

                        // Check for valid year ranges (e.g., must be after a reasonable minimum year)
                        if (startYear < 1900 || endYear < 1900 || startYear > DateTime.Now.Year + 1 || endYear > DateTime.Now.Year + 1)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_EDU_YEAR_RANGE",
                                $"Education record year is outside a valid range. Start: {startYear}, End: {endYear}.");
                            return BadRequest(new { message = "Problem saving in Data Sheet 1. start year cannot be after end year" });
                        }

                        // Check if the Start Year is GREATER than the End Year
                        if (startYear > endYear)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_EDU_DATE_ORDER",
                                $"Education record start year ({startYear}) is after end year ({endYear}).");
                            return BadRequest(new { message = "Problem saving in Data Sheet 1. start year cannot be after end year" });
                        }
                    }
                }
            }

            // 6. Voluntary Work Date Order Check (Similar date consistency check)
            if (dto.VoluntaryWorks != null)
            {
                foreach (var vwDto in dto.VoluntaryWorks)
                {
                    if (vwDto.DateFrom.HasValue && vwDto.DateTo.HasValue)
                    {
                        if (vwDto.DateFrom.Value.Date > vwDto.DateTo.Value.Date)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_VW_DATE_ORDER",
                                $"Voluntary Work record start date ({vwDto.DateFrom.Value.ToShortDateString()}) is after end date ({vwDto.DateTo.Value.ToShortDateString()}).");
                            return BadRequest(new { message = "Problem saving in Data Sheet num. Voluntary Work error: Start date cannot be after the end date." });
                        }
                    }
                }
            }

            // 7. Training Date Order Check
            if (dto.Trainings != null)
            {
                foreach (var tDto in dto.Trainings)
                {
                    if (tDto.DateFrom.HasValue && tDto.DateTo.HasValue)
                    {
                        if (tDto.DateFrom.Value.Date > tDto.DateTo.Value.Date)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_TRAINING_DATE_ORDER",
                                $"Training record start date ({tDto.DateFrom.Value.ToShortDateString()}) is after end date ({tDto.DateTo.Value.ToShortDateString()}).");
                            return BadRequest(new { message = "Problem saving in Data Sheet num. Start date cannot be after the end date." });
                        }
                    }
                }
            }

            if (dto.WorkExperiences != null)
            {
                foreach (var wDto in dto.WorkExperiences)
                {
                    // Check if both dates have values
                    if (wDto.DateFrom.HasValue && wDto.DateTo.HasValue)
                    {
                        if (wDto.DateFrom.Value.Date > wDto.DateTo.Value.Date)
                        {
                            // 1. FIXED Log Tag and Message to reflect Work Experience
                            LogPdsAction("PDS_VALIDATION_FAILURE_WORKEXP_DATE_ORDER",
                                $"Work Experience record start date ({wDto.DateFrom.Value.ToShortDateString()}) is after end date ({wDto.DateTo.Value.ToShortDateString()}).");

                            // 2. FIXED Return message for clarity
                            return BadRequest(new { message = "Problem saving Work Experience: Start date cannot be after the end date." });
                        }
                    }
                }
            }

            if (dto.EducationRecords != null)
            {
                foreach (var eduDto in dto.EducationRecords)
                {
                    // 1. Check if the record is empty (no School Name, no Degree, and no Attendance Years)
                    // Note: Since SchoolName and Degree are nullable, we must use IsNullOrWhiteSpace check.
                    bool isRecordEmpty = string.IsNullOrWhiteSpace(eduDto.SchoolName)
                                         && string.IsNullOrWhiteSpace(eduDto.Degree)
                                         && !eduDto.AttendanceFrom.HasValue
                                         && !eduDto.AttendanceTo.HasValue
                                         && string.IsNullOrWhiteSpace(eduDto.Level); // If the level is also empty

                    // Skip validation if the entire record is empty (this should only happen if the frontend sends a fixed 5-item array)
                    if (isRecordEmpty)
                    {
                        continue;
                    }

                    // --- EXISTING YEAR RANGE CHECKS ---
                    if (eduDto.AttendanceFrom.HasValue && eduDto.AttendanceTo.HasValue)
                    {
                        int startYear = eduDto.AttendanceFrom.Value;
                        int endYear = eduDto.AttendanceTo.Value;

                        // Check for valid year ranges (e.g., must be after a reasonable minimum year)
                        if (startYear < 1900 || endYear < 1900 || startYear > DateTime.Now.Year + 1 || endYear > DateTime.Now.Year + 1)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_EDU_YEAR_RANGE",
                                $"Education record year is outside a valid range. Start: {startYear}, End: {endYear}.");
                            return BadRequest(new { message = "Problem saving in Data Sheet 1. The academic year must be within a valid range." });
                        }

                        // Check if the Start Year is GREATER than the End Year
                        if (startYear > endYear)
                        {
                            LogPdsAction("PDS_VALIDATION_FAILURE_EDU_DATE_ORDER",
                                $"Education record start year ({startYear}) is after end year ({endYear}).");
                            return BadRequest(new { message = "Problem saving in Data Sheet 1. Start year cannot be after end year." });
                        }
                    }
                }
            }

            // ----------------------------------------------------
            // ✅ DOCUMENT FILE HANDLING & MAPPING 
            // ----------------------------------------------------

            // 1. Handle Birth Certificate
            string existingBirthCertUrl = person.BirthCertificatePath != null
                ? Url.Content(person.BirthCertificatePath)
                : null;

            // Determine if the client sent a signal to clear the file (null/empty key OR explicit "DELETE_FILE")
            bool isBirthCertClearSignal = string.IsNullOrWhiteSpace(dto.BirthCertificateFileKey) ||
                                          dto.BirthCertificateFileKey.Equals("DELETE_FILE", StringComparison.OrdinalIgnoreCase);

            if (isBirthCertClearSignal)
            {
                // Only clear the path if it wasn't null already (optimization)
                if (person.BirthCertificatePath != null)
                {

                    person.BirthCertificatePath = null;
                    //person.BirthCertificateOriginalName = null;
                    LogPdsAction("PDS_DOC_ACTION_BIRTH_CERT", "Birth Certificate path cleared (via DELETE or empty key).");
                    //await _fileService.DeleteFile(person.BirthCertificatePath); 

                }
                person.BirthCertificatePath = null;
            }
            // 2. Only proceed with existing file check or upload if the key is NOT a clear signal
            else
            {
                // ⭐ CRITICAL FIX LOCATION: Case 2: No Change / Retained File ⭐
                if (!string.IsNullOrWhiteSpace(existingBirthCertUrl) && dto.BirthCertificateFileKey.Equals(existingBirthCertUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // This is the line that preserves the original name when the user doesn't change the file
                    //person.BirthCertificateOriginalName = dto.BirthCertificateOriginalName;
                    LogPdsAction("PDS_DOC_NO_CHANGE_BIRTH_CERT", "Birth Certificate key matches existing path. No change needed.");
                }
                else
                {
                    try
                    {
                        // This path is for new uploads (new temporary GUID)
                        string permanentPath = await _fileService.FinalizeUpload(dto.BirthCertificateFileKey, id);

                        person.BirthCertificatePath = permanentPath;
                        // This is the line that saves the name when a new file is uploaded
                        //person.BirthCertificateOriginalName = dto.BirthCertificateOriginalName;

                        LogPdsAction("PDS_DOC_UPLOAD_BIRTH_CERT", $"Birth Certificate path set: {permanentPath}.");
                    }
                    catch (NotSupportedException nse)
                    {
                        LogPdsAction("PDS_DOC_FAILURE_BIRTH_CERT", $"Disallowed file type: {nse.Message}");
                        return BadRequest(new { message = nse.Message });
                    }
                    catch (Exception ex)
                    {
                        LogPdsAction("PDS_DOC_FAILURE_BIRTH_CERT", $"Failed to process Birth Certificate file: {ex.Message}");
                        return StatusCode(500, new { message = "Error processing Birth Certificate file.", detail = ex.Message });
                    }
                }
            }
            // 3. Handle Marriage Certificate
            // Calculate the expected URL of the existing file for correct "no change" comparison
            string existingMarriageCertUrl = person.MarriageCertificatePath != null
                ? Url.Content(person.MarriageCertificatePath)
                : null;

            bool isMarriageCertClearSignal = string.IsNullOrWhiteSpace(dto.MarriageCertificateFileKey) ||
                                             dto.MarriageCertificateFileKey.Equals("DELETE_FILE", StringComparison.OrdinalIgnoreCase);

            if (isMarriageCertClearSignal)
            {
                if (person.MarriageCertificatePath != null)
                {

                    person.MarriageCertificatePath = null;
                    //person.MarriageCertificateOriginalName = null;
                    LogPdsAction("PDS_DOC_ACTION_MARRIAGE_CERT", "Marriage Certificate path cleared (via DELETE or empty key).");
                }

                person.MarriageCertificatePath = null; // Force the path to null here too, for safety.

                if (dto.CivilStatusID == 2 && person.MarriageCertificatePath == null)
                {

                    LogPdsAction("PDS_DOC_WARNING_MARRIAGE_STATUS", "Civil status is 'Married' but Marriage certificate path was cleared.");
                }
            }

            // 4. Only proceed with existing file check or upload if the key is NOT a clear signal
            else
            {
                if (!string.IsNullOrWhiteSpace(existingMarriageCertUrl) && dto.MarriageCertificateFileKey.Equals(existingMarriageCertUrl, StringComparison.OrdinalIgnoreCase))
                {
                    LogPdsAction("PDS_DOC_NO_CHANGE_MARRIAGE_CERT", "Marriage Certificate key matches existing path. No change needed.");
                }
                else
                {
                    try
                    {
                        string permanentPath = await _fileService.FinalizeUpload(dto.MarriageCertificateFileKey, id);
                        person.MarriageCertificatePath = permanentPath;
                        person.MarriageCertificateOriginalName = dto.MarriageCertificateOriginalName;
                        LogPdsAction("PDS_DOC_UPLOAD_MARRIAGE_CERT", $"Marriage Certificate path set: {permanentPath}.");
                    }
                    catch (NotSupportedException nse)
                    {
                        LogPdsAction("PDS_DOC_FAILURE_MARRIAGE_CERT", $"Disallowed file type: {nse.Message}");
                        return BadRequest(new { message = nse.Message });
                    }
                    catch (Exception ex)
                    {
                        LogPdsAction("PDS_DOC_FAILURE_MARRIAGE_CERT", $"Failed to process Marriage Certificate file: {ex.Message}");
                        return StatusCode(500, new { message = "Error processing Marriage Certificate file.", detail = ex.Message });
                    }
                }
            }


            // Map simple fields
            person.PersonID = id;
            person.CsIdNo = dto.CsIdNo;
            person.Surname = dto.Surname;
            person.FirstName = dto.FirstName;
            person.MiddleName = dto.MiddleName;
            person.NameExtension = dto.NameExtension;

            person.DateOfBirth = dto.DateOfBirth;
            person.PlaceOfBirth = dto.PlaceOfBirth;

            person.SexID = dto.SexID;
            person.CivilStatusID = dto.CivilStatusID;
            person.Citizenship = dto.Citizenship;

            person.HeightCM = dto.HeightCM;
            person.WeightKG = dto.WeightKG;
            person.BloodType = dto.BloodType;

            person.GsisID = dto.GsisID;
            person.PagibigID = dto.PagibigID;
            person.PhilhealthID = dto.PhilhealthID;
            person.SssNo = dto.SssNo;
            person.Tin = dto.Tin;
            person.AgencyEmployeeNo = dto.AgencyEmployeeNo;

            person.ResHouseBlockLot= dto.ResHouseBlockLot;
            person.ResStreet = dto.ResStreet;
            person.ResBarangay = dto.ResBarangay;
            person.ResCityMunicipality = dto.ResCityMunicipality;
            person.ResProvince = dto.ResProvince;
            person.ResZip = dto.ResZip;

            person.PermHouseBlockLot = dto.PermHouseBlockLot;
            person.PermStreet = dto.PermStreet;
            person.PermBarangay = dto.PermBarangay;
            person.PermCityMunicipality = dto.PermCityMunicipality;
            person.PermProvince = dto.PermProvince;
            person.PermZip = dto.PermZip;

            person.TelephoneNo = dto.TelephoneNo;
            person.MobileNo = dto.MobileNo;
            person.Email = dto.Email;

            person.BirthCertificateOriginalName = dto.BirthCertificateOriginalName;
            person.MarriageCertificateOriginalName = dto.MarriageCertificateOriginalName;

            person.DepartmentID = dto.DepartmentID;
            person.Designation = dto.Designation;

            // Spouse
            person.SpouseSurname = dto.SpouseSurname;
            person.SpouseFirstName = dto.SpouseFirstName;
            person.SpouseMiddleName = dto.SpouseMiddleName;
            person.SpouseNameExtension = dto.SpouseNameExtension;
            person.SpouseOccupation = dto.SpouseOccupation;
            person.SpouseEmployer = dto.SpouseEmployer;
            person.SpouseBusinessAddress = dto.SpouseBusinessAddress;
            person.SpouseTelephone = dto.SpouseTelephone;

            // Father
            person.FatherSurname = dto.FatherSurname;
            person.FatherFirstName = dto.FatherFirstName;
            person.FatherMiddleName = dto.FatherMiddleName;
            person.FatherNameExtension = dto.FatherNameExtension;

            // Mother
            person.MotherSurname = dto.MotherSurname;
            person.MotherFirstName = dto.MotherFirstName;
            person.MotherMiddleName = dto.MotherMiddleName;
            person.MotherNameExtension = dto.MotherNameExtension;





            // Children
            person.Children.Clear();
            foreach (var childDto in dto.Children)
            {
                person.Children.Add(new Child
                {
                    PersonID = id,
                    FullName = childDto.FullName,
                    DateOfBirth = childDto.DateOfBirth
                });
            }

            // Education
            person.EducationRecords.Clear();
            foreach (var eduDto in dto.EducationRecords)
            {
                // 🔍 Find the specific ID for THIS specific row's level name
                var levelId = await _db.EducationLevels
                    .Where(e => e.Display_Name == eduDto.Level)
                    .Select(e => e.Display_Name)
                    .FirstOrDefaultAsync();

                person.EducationRecords.Add(new Education
                {
                    PersonID = id,
                    Level = levelId, // Now specific to this record (e.g., 1 for Elementary, 5 for College)
                    SchoolName = eduDto.SchoolName,
                    Degree = eduDto.Degree,
                    AttendanceFrom = eduDto.AttendanceFrom,
                    AttendanceTo = eduDto.AttendanceTo,
                    HighestLevel = eduDto.HighestLevel,
                    YearGraduated = eduDto.YearGraduated,
                    Honors = eduDto.Honors,
                    OrderIndex = eduDto.OrderIndex,
                });
            }

            // Voluntary Work
            person.VoluntaryWorks.Clear();
            foreach (var vwDto in dto.VoluntaryWorks)
            {
                person.VoluntaryWorks.Add(new VoluntaryWork
                {
                    PersonID = id,
                    Organization = vwDto.Organization,
                    DateFrom = vwDto.DateFrom,
                    DateTo = vwDto.DateTo,
                    NumberOfHours = vwDto.NumberOfHours,
                    Position = vwDto.Position
                });
            }

            // Trainings
            person.Trainings.Clear();
            foreach (var tDto in dto.Trainings)
            {
                person.Trainings.Add(new Training
                {
                    PersonID = id,
                    Title = tDto.Title,
                    DateFrom = tDto.DateFrom,
                    DateTo = tDto.DateTo,
                    NumberOfHours = tDto.NumberOfHours,
                    TypeOfLD = tDto.TypeOfLD,
                    ConductedBy = tDto.ConductedBy
                });
            }

            // Skills & Hobbies
            person.SkillsHobbies.Clear();
            foreach (var sDto in dto.SkillsHobbies)
            {
                person.SkillsHobbies.Add(new SkillHobby
                {
                    PersonID = id,
                    SkillOrHobby = sDto.SkillOrHobby
                });
            }

            // Distinctions
            person.Distinctions.Clear();
            foreach (var dDto in dto.Distinctions)
            {
                person.Distinctions.Add(new Distinction
                {
                    PersonID = id,
                    DistinctionName = dDto.Distinction
                });
            }

            // Memberships
            person.Memberships.Clear();
            foreach (var mDto in dto.Memberships)
            {
                person.Memberships.Add(new Membership
                {
                    PersonID = id,
                    OrganizationName = mDto.OrganizationName
                });
            }

            // Civil Service
            person.CivilServiceEligibilities.Clear();
            foreach (var eDto in dto.CivilServiceEligibilities)
            {
                person.CivilServiceEligibilities.Add(new CivilServiceEligibility
                {
                    PersonID = id,
                    CareerService = eDto.CareerService,
                    Rating = eDto.Rating,
                    DateOfExamination = eDto.DateOfExamination,
                    PlaceOfExamination = eDto.PlaceOfExamination,
                    LicenseNumber = eDto.LicenseNumber,
                    LicenseValidity = eDto.LicenseValidity,
                    DateReleased = eDto.DateReleased
                });
            }

            // Work Experiences
            person.WorkExperiences.Clear();
            foreach (var wDto in dto.WorkExperiences)
            {
                person.WorkExperiences.Add(new WorkExperience
                {
                    PersonID = id,
                    DateFrom = wDto.DateFrom ?? DateTime.MinValue,
                    DateTo = wDto.DateTo,
                    PositionTitle = wDto.PositionTitle,
                    DepartmentAgencyCompany = wDto.DepartmentAgencyCompany,
                    MonthlySalary = wDto.MonthlySalary,
                    SalaryGradeStep = wDto.SalaryGradeStep,
                    StatusOfAppointment = wDto.StatusOfAppointment,
                    IsGovernmentService = wDto.IsGovernmentService,
                    Description = wDto.Description
                });
            }

            // PDS Section IV
            if (dto.PdsSectionIV != null)
            {
                // 1. Check if the record already exists for this PersonID
                if (person.PdsSectionIV == null)
                {
                    // Insert new record if it doesn't exist
                    person.PdsSectionIV = new PdsSectionIV { PersonID = id };
                    // EF Core will automatically track and add this entity
                }

                var pdsIV = person.PdsSectionIV;

                // --- 34. Relatives ---
                pdsIV.Q34a_RelatedWithin3rd = dto.PdsSectionIV.Q34a_RelatedWithin3rd;
                pdsIV.Q34b_RelatedWithin4th = dto.PdsSectionIV.Q34b_RelatedWithin4th;
                pdsIV.Q34_Details = dto.PdsSectionIV.Q34_Details;

                // --- 35. Administrative/Criminal Charges ---
                pdsIV.Q35a_AdminOffense = dto.PdsSectionIV.Q35a_AdminOffense;
                pdsIV.Q35b_CriminallyCharged = dto.PdsSectionIV.Q35b_CriminallyCharged;

                // Use the specific detail fields for clean mapping
                pdsIV.Q35a_Details = dto.PdsSectionIV.Q35a_AdminDetails;
                pdsIV.Q35b_Details = dto.PdsSectionIV.Q35b_CriminalDetails;

                pdsIV.Q35b_DateFiled = dto.PdsSectionIV.Q35b_DateFiled;
                pdsIV.Q35b_Status = dto.PdsSectionIV.Q35b_Status;


                // --- 36. Convicted of Crime ---
                pdsIV.Q36_Convicted = dto.PdsSectionIV.Q36_Convicted;
                pdsIV.Q36_Details = dto.PdsSectionIV.Q36_Details;

                // --- 37. Separation from Service ---
                pdsIV.Q37_Separated = dto.PdsSectionIV.Q37_Separated;
                pdsIV.Q37_Details = dto.PdsSectionIV.Q37_Details;

                // --- 38. Election History ---
                pdsIV.Q38a_Candidate = dto.PdsSectionIV.Q38a_Candidate;
                pdsIV.Q38b_ResignedForCampaign = dto.PdsSectionIV.Q38b_ResignedForCampaign;
                pdsIV.Q38a_Details = dto.PdsSectionIV.Q38a_Details;
                pdsIV.Q38b_Details = dto.PdsSectionIV.Q38b_Details;

                // --- 39. Immigrant/Permanent Resident Status & Dual Citizenship ---
                pdsIV.Q39_Country = dto.PdsSectionIV.Q39_Country;
                pdsIV.Q39_Details_Country = dto.PdsSectionIV.Q39_Details_Country;


                // --- 40. Other Information ---
                pdsIV.Q40a_IndigenousGroup = dto.PdsSectionIV.Q40a_IndigenousGroup;
                pdsIV.Q40a_Details = dto.PdsSectionIV.Q40a_Details;
                pdsIV.Q40b_Disability = dto.PdsSectionIV.Q40b_Disability;
                pdsIV.Q40b_Details_IDNo = dto.PdsSectionIV.Q40b_Details_IDNo;
                pdsIV.Q40c_SoloParent = dto.PdsSectionIV.Q40c_SoloParent;
                pdsIV.Q40c_Details_IDNo = dto.PdsSectionIV.Q40c_Details_IDNo;
            }

            // Q. 41: References
            person.PersonalInformationReferences.Clear();
            foreach (var rDto in dto.References)
            {
                person.PersonalInformationReferences.Add(new PersonalInformationReference
                {
                    PersonID = id,
                    Name = rDto.Name,
                    OfficeOrResidentialAddress = rDto.OfficeOrResidentialAddress,
                    ContactNoOrEmail = rDto.ContactNoOrEmail
                });
            }

            // Q. 42: Declaration and ID Details
            if (dto.Declaration != null)
            {
                var declarationDto = dto.Declaration;

                // 1. Upsert (Find or Create) the Declaration record
                if (person.Declaration == null)
                {
                    person.Declaration = new PdsSectionVDeclaration { PersonID = id };
                }

                var declaration = person.Declaration;

                // 2. Handle File Uploads for Declaration (Photo, Signature, Thumbmark)
                try
                {
                    // Photo
                    declaration.PhotoPath = await HandlePdsVFile(id, declarationDto.PhotoFileKey, "Photo", declaration.PhotoPath);

                    // ⭐ NEW LINE: Map the Original Filename for the Photo ⭐
                    declaration.PhotoOriginalName = declarationDto.PhotoOriginalName;

                    //// Signature
                    //declaration.SignaturePath = await HandlePdsVFile(id, declarationDto.SignatureFileKey, "Signature", declaration.SignaturePath);

                    // Right Thumbmark
                    declaration.RightThumbmarkPath = await HandlePdsVFile(id, declarationDto.RightThumbmarkFileKey, "Thumbmark", declaration.RightThumbmarkPath);
                }
                catch (Exception ex) when (ex is NotSupportedException || ex is IOException)
                {
                    LogPdsAction("PDS_DOC_FAILURE_DECLARATION_FILES", $"File processing error for PDS V: {ex.Message}");
                    return StatusCode(500, new { message = "Error processing declaration files.", detail = ex.Message });
                }


                // 3. Map PDS V fields
                declaration.DeclarationID = declarationDto.DeclarationID;
                declaration.GovernmentIssuedID = declarationDto.GovernmentIssuedID;
                declaration.LicensePassportIDNo = declarationDto.LicensePassportIDNo;

                declaration.SignaturePath = declarationDto.Signature;
                declaration.AdminSignature = declarationDto.AdminSignature;

                declaration.DateOfIssuance = declarationDto.DateOfIssuance;
                declaration.PlaceOfIssuance = declarationDto.PlaceOfIssuance;
                declaration.DateAccomplished = declarationDto.DateAccomplished;



                // Only update DateSchedule if the current one is null/min and the DTO has a value
                if (!declaration.DateSchedule.HasValue && declarationDto.DateSchedule.HasValue)
                {
                    declaration.DateSchedule = declarationDto.DateSchedule;
                }
            }

            async Task<string?> HandlePdsVFile(int personId, string? fileKey, string docName, string? currentPath)
            {
                if (string.IsNullOrWhiteSpace(fileKey))
                {
                    // When the frontend sends a null/empty key, it means the field was cleared 
                    // OR no file was ever selected/uploaded. If currentPath is not null, 
                    // we assume the intent is to clear the existing file.
                    if (currentPath != null)
                    {
                        LogPdsAction("PDS_DOC_ACTION_" + docName.ToUpper(), $"{docName} path implicitly cleared via empty key.");
                    }
                    return null; // Force the database path to be cleared (NULL)
                }

                // Calculate the expected URL of the existing file for correct "no change" comparison
                string? existingUrl = currentPath != null
                    ? Url.Content(currentPath) // Assuming Url.Content is accessible and correctly converts the DB path to the public URL
                    : null;

                // --- SCENARIO 3: Explicit Deletion ---
                if (fileKey.Equals("DELETE_FILE", StringComparison.OrdinalIgnoreCase))
                {
                    LogPdsAction("PDS_DOC_ACTION_" + docName.ToUpper(), $"{docName} path explicitly cleared.");
                    return null;
                }

                // ⭐ SCENARIO 2: Existing File (NO CHANGE) - CRITICAL FIX ⭐
                // If the DTO key (which is the URL from the front-end) matches the existing URL, skip FinalizeUpload.
                if (!string.IsNullOrWhiteSpace(existingUrl) && fileKey.Equals(existingUrl, StringComparison.OrdinalIgnoreCase))
                {
                    LogPdsAction("PDS_DOC_NO_CHANGE_" + docName.ToUpper(), $"{docName} key matches existing path. No change needed.");
                    return currentPath; // Return the existing permanent DB path
                }

                // --- SCENARIO 1: New File Upload (Temporary Key) ---
                // If it reached here, fileKey must be a temporary GUID.
                try
                {
                    string permanentPath = await _fileService.FinalizeUpload(fileKey, personId);
                    LogPdsAction("PDS_DOC_UPLOAD_" + docName.ToUpper(), $"{docName} path set: {permanentPath}.");
                    return permanentPath;
                }
                // IMPORTANT: Add specific catches here or rely on the outer try/catch to handle file service errors
                catch (Exception ex)
                {
                    // Re-throw the exception so the outer controller catch block (which you provided) can handle the HTTP 500 return
                    throw new IOException($"Error finalizing {docName} file: {ex.Message}", ex);
                }
            }

            // Fetch Department Name based on DepartmentID
            var department = await _db.Department
                .Where(d => d.DepartmentID == dto.DepartmentID)
                .Select(d => d.DepartmentName)
                .FirstOrDefaultAsync();



            //Fetch Gender Name based on GenderID
            //fetch Civil Status Name based on CivilStatusID

            // employee ID Application
            var idApplication = await _db.EnrollmentRegistries
                .FirstOrDefaultAsync(p => p.PersonID == id);

            if (idApplication == null)
            {
                idApplication = new EnrollmentRegistryID();
                _db.EnrollmentRegistries.Add(idApplication);
                idApplication.Status = 0; //status Pending
                LogPdsAction("ID_APP_RECORD_INSERT_INIT", "New EmployeeIDApplication record created.");
            }


            if (idApplication.DateSchedule == null)
            {
                idApplication.PersonID = id;
                idApplication.LastName = dto.Surname;
                idApplication.FirstName = dto.FirstName;
                idApplication.MiddleName = dto.MiddleName;
                idApplication.BirthDate = dto.DateOfBirth;
                idApplication.EmployeeID = dto.AgencyEmployeeNo;

                idApplication.DepartmentID = dto.DepartmentID;
                idApplication.DepartmentName = department;


                idApplication.Designation = dto.Designation;

                //await _db.SaveChangesAsync();
            }

            // 5. UPDATE FIELDS (Removed the if DateSchedule == null check)
            // This ensures Dept, Gender, and Names update even if a schedule exists.
            idApplication.LastName = dto.Surname;
            idApplication.FirstName = dto.FirstName;
            idApplication.MiddleName = dto.MiddleName;
            idApplication.BirthDate = dto.DateOfBirth;
            idApplication.EmployeeID = dto.AgencyEmployeeNo;

            // Mapping the Lookup Values
            idApplication.DepartmentID = dto.DepartmentID;
            idApplication.DepartmentName = department;
            idApplication.Designation = dto.Designation;
            //idApplication.PrintDesignation = false;

            // --- CRITICAL SAVE POINT ---
            try
            {
                await _db.SaveChangesAsync();
                LogPdsAction("ID_APP_RECORD_SAVED", $"EmployeeIDApplication updated for {idApplication.FirstName} in {department}.");
            }
            catch (Exception ex)
            {
                LogPdsAction("ID_APP_RECORD_SAVE_FAILURE", $"Failed to save EmployeeIDApplication: {ex.Message}");
                return StatusCode(500, "Error saving ID Application details.");
            }

            LogPdsAction("PDS_COLLECTIONS_MAPPED", $"Mapped {dto.WorkExperiences.Count} Work Experiences and {dto.Children.Count} Children.");
            return Ok(new
            {
                personId = person.PersonID,
                message = "Personal Data Sheet successfully updated."
            });

        }

        /// <summary>
        /// Submits a preferred date for an ID card capture/issuance schedule.
        /// </summary>
        [HttpPost("my-schedule/request")]
        public async Task<IActionResult> RequestSchedule([FromBody] MyScheduleRequest request)
        {
            var userIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var app = await _db.EnrollmentRegistries
                    .FirstOrDefaultAsync(a => a.PersonID == userId);
            if (app == null) return NotFound("Application not found.");

            // Set Status to 7 (Schedule for Approval)
            app.DateSchedule = request.PreferredDate;
            app.Status = 7;

            _db.ApplicationLogs.Add(new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                LogDescription = "SCHEDULE_REQUESTED_BY_EMPLOYEE",
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"User requested schedule for {request.PreferredDate:yyyy-MM-dd}. Status set to 7."
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Schedule request submitted for HR approval." });
        }

        /// <summary>
        /// Retrieves the current schedule date and application status for the authenticated user.
        /// </summary>
        [HttpGet("my-schedule/date")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetScheduleDate()
        {
            // 1. Identify User from Token
            var userIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            // 2. Fetch from Unified Registry
            // We target the EnrollmentRegistries table as the single source of truth
            var registrationData = await _db.EnrollmentRegistries
                .AsNoTracking()
                .Where(r => r.PersonID == userId)
                .Select(r => new
                {
                    r.DateSchedule, // The appointment date
                    r.Status,       // The overall workflow status (e.g., 1=Scheduled, 7=Pending)
                    r.ApplicationCode
                })
                .FirstOrDefaultAsync();

            if (registrationData == null)
            {
                return NotFound(new { message = "Enrollment record not found for this user." });
            }

            // 3. Return anonymous object
            return Ok(new
            {
                dateSchedule = registrationData.DateSchedule,
                status = registrationData.Status,
                applicationCode = registrationData.ApplicationCode
            });
        }

        /// <summary>
        /// Generates and returns a formal PDF document of the Personal Data Sheet (PDS).
        /// </summary>
        [HttpGet("pdf/{id}")]
        public async Task<ActionResult> GetPdsPdf(int id)
        {
            int? userId = null;
            string username = User.FindFirstValue("username"); // Get authenticated username or default

            // Helper function to log actions (assuming it exists in your controller)
            void LogPdsAction(string description, string specificNote = null)
            {
                // Implementation omitted for brevity, but should mirror your LogPdsAction from GetC1
                // ... logging implementation ...
            }

            // 1. Retrieve the authenticated user's ID for logging
            if (User.Identity.IsAuthenticated && int.TryParse(User.FindFirstValue("id"), out int authenticatedId))
            {
                userId = authenticatedId;
            }




            LogPdsAction("PDS_PDF_ATTEMPT", $"Attempting to generate PDF for PersonID: {id}.");

            // 2. Fetch the data with all required related entities (Includes)
            var person = await _db.PersonalInformations
                .Include(p => p.Children)
                .Include(p => p.EducationRecords)
                .Include(p => p.VoluntaryWorks)
                .Include(p => p.Trainings)
                .Include(p => p.SkillsHobbies)
                .Include(p => p.Distinctions)
                .Include(p => p.Memberships)
                .Include(p => p.CivilServiceEligibilities)
                .Include(p => p.WorkExperiences)
                .Include(p => p.PdsSectionIV)
                .Include(p => p.PersonalInformationReferences)
                .Include(p => p.Declaration)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonID == id);

            // Fetch Genders
            var genders = await _db.Gender.AsNoTracking().ToListAsync();
            var genderMap = genders.ToDictionary(g => g.GenderID, g => g.GenderName);

            // Fetch Civil Statuses
            var statuses = await _db.CivilStatus.AsNoTracking().ToListAsync();
            var statusMap = statuses.ToDictionary(s => s.CivilStatusID, s => s.StatusName);

            // 3. Handle 'Not Found' case
            if (person == null)
            {
                LogPdsAction("PDS_PDF_FAILURE", $"PDS with PersonID: {id} not found for PDF generation.");
                // await _db.SaveChangesAsync(); // Uncomment if you need to save the log entry here
                return NotFound();
            }

            // 4. Map the Model to the DTO (Reuse mapping logic from GetC1)
            var dto = new PersonalInformationDto
            {
                // --- Personal Information (Section I) ---
                PersonID = person.PersonID,
                CsIdNo = person.CsIdNo,
                Surname = person.Surname,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                NameExtension = person.NameExtension,
                DateOfBirth = person.DateOfBirth,
                PlaceOfBirth = person.PlaceOfBirth,
                SexID = person.SexID,
                CivilStatusID = person.CivilStatusID,
                BirthCertificateFileKey = person.BirthCertificatePath,
                MarriageCertificateFileKey = person.MarriageCertificatePath,
                Citizenship = person.Citizenship,
                HeightCM = person.HeightCM,
                WeightKG = person.WeightKG,
                BloodType = person.BloodType,
                GsisID = person.GsisID,
                PagibigID = person.PagibigID,
                PhilhealthID = person.PhilhealthID,
                SssNo = person.SssNo,
                Tin = person.Tin,
                AgencyEmployeeNo = person.AgencyEmployeeNo,

                ResHouseBlockLot = person.ResHouseBlockLot,
                ResStreet = person.ResStreet,
                ResBarangay = person.ResBarangay,
                ResCityMunicipality = person.ResCityMunicipality,
                ResProvince = person.ResProvince,
                ResZip = person.ResZip,

                PermHouseBlockLot = person.PermHouseBlockLot,
                PermStreet = person.PermStreet,
                PermBarangay = person.PermBarangay,
                PermCityMunicipality = person.PermCityMunicipality,
                PermProvince = person.PermProvince,
                PermZip = person.PermZip,

                TelephoneNo = person.TelephoneNo,
                MobileNo = person.MobileNo,
                Email = person.Email,

                // --- Family Background (Section II) ---
                SpouseSurname = person.SpouseSurname,
                SpouseFirstName = person.SpouseFirstName,
                SpouseMiddleName = person.SpouseMiddleName,
                SpouseNameExtension = person.SpouseNameExtension,
                SpouseOccupation = person.SpouseOccupation,
                SpouseEmployer = person.SpouseEmployer,
                SpouseBusinessAddress = person.SpouseBusinessAddress,
                SpouseTelephone = person.SpouseTelephone,
                FatherSurname = person.FatherSurname,
                FatherFirstName = person.FatherFirstName,
                FatherMiddleName = person.FatherMiddleName,
                FatherNameExtension = person.FatherNameExtension,
                MotherSurname = person.MotherSurname,
                MotherFirstName = person.MotherFirstName,
                MotherMiddleName = person.MotherMiddleName,
                MotherNameExtension = person.MotherNameExtension,

                // --- Collections Mapping ---
                Children = person.Children.Select(c => new ChildDto
                {
                    ChildID = c.ChildId,
                    PersonID = c.PersonID,
                    FullName = c.FullName,
                    DateOfBirth = c.DateOfBirth
                }).ToList(),

                // Map other collections (EducationRecords, VoluntaryWorks, Trainings, etc.) here...
                EducationRecords = person.EducationRecords.OrderBy(e => e.OrderIndex).Select(e => new EducationDto
                {
                    Level = e.Level,
                    SchoolName = e.SchoolName,
                    Degree = e.Degree,
                    AttendanceFrom = e.AttendanceFrom,
                    AttendanceTo = e.AttendanceTo,
                    HighestLevel = e.HighestLevel,
                    YearGraduated = e.YearGraduated,
                    Honors = e.Honors,
                    OrderIndex = e.OrderIndex
                }).ToList(),

                VoluntaryWorks = person.VoluntaryWorks.Select(v => new VoluntaryWorkDTO
                {
                    WorkId = v.WorkId,
                    PersonID = v.PersonID,
                    Organization = v.Organization,
                    DateFrom = v.DateFrom,
                    DateTo = v.DateTo,
                    NumberOfHours = v.NumberOfHours,
                    Position = v.Position
                }).ToList(),

                Trainings = person.Trainings.Select(t => new TrainingDTO
                {
                    TrainingId = t.TrainingId,
                    PersonID = t.PersonID,
                    Title = t.Title,
                    DateFrom = t.DateFrom,
                    DateTo = t.DateTo,
                    NumberOfHours = t.NumberOfHours,
                    TypeOfLD = t.TypeOfLD,
                    ConductedBy = t.ConductedBy
                }).ToList(),

                SkillsHobbies = person.SkillsHobbies.Select(s => new SkillHobbyDTO { SkillId = s.SkillId, PersonID = s.PersonID, SkillOrHobby = s.SkillOrHobby }).ToList(),
                Distinctions = person.Distinctions.Select(d => new DistinctionDTO { DistinctionId = d.DistinctionId, PersonID = d.PersonID, Distinction = d.DistinctionName }).ToList(),
                Memberships = person.Memberships.Select(m => new MembershipDTO { MembershipId = m.MembershipId, PersonID = m.PersonID, OrganizationName = m.OrganizationName }).ToList(),

                CivilServiceEligibilities = person.CivilServiceEligibilities.Select(e => new CivilServiceEligibilityDTO
                {
                    EligibilityID = e.EligibilityID,
                    PersonID = e.PersonID,
                    CareerService = e.CareerService,
                    Rating = e.Rating,
                    DateOfExamination = e.DateOfExamination,
                    PlaceOfExamination = e.PlaceOfExamination,
                    LicenseNumber = e.LicenseNumber,
                    LicenseValidity = e.LicenseValidity,
                    DateReleased = e.DateReleased
                }).ToList(),

                WorkExperiences = person.WorkExperiences.Select(w => new WorkExperienceDTO
                {
                    WorkExperienceID = w.WorkExperienceId,
                    PersonID = w.PersonID,
                    DateFrom = w.DateFrom,
                    DateTo = w.DateTo,
                    PositionTitle = w.PositionTitle,
                    DepartmentAgencyCompany = w.DepartmentAgencyCompany,
                    MonthlySalary = w.MonthlySalary,
                    SalaryGradeStep = w.SalaryGradeStep,
                    StatusOfAppointment = w.StatusOfAppointment,
                    IsGovernmentService = w.IsGovernmentService,
                    Description = w.Description
                }).ToList(),

                // --- PdsSectionIV (Other Information) ---
                PdsSectionIV = person.PdsSectionIV != null ? new PdsSectionIVDto
                {
                    PdsSectionIVId = person.PdsSectionIV.PdsSectionIVId,

                    // --- 34. Relatives (Model -> DTO) ---
                    Q34a_RelatedWithin3rd = person.PdsSectionIV.Q34a_RelatedWithin3rd,
                    Q34b_RelatedWithin4th = person.PdsSectionIV.Q34b_RelatedWithin4th,
                    Q34_Details = person.PdsSectionIV.Q34_Details,

                    // --- 35. Administrative/Criminal Charges ---
                    Q35a_AdminOffense = person.PdsSectionIV.Q35a_AdminOffense,
                    Q35b_CriminallyCharged = person.PdsSectionIV.Q35b_CriminallyCharged,

                    Q35a_AdminDetails = person.PdsSectionIV.Q35a_Details,
                    Q35b_CriminalDetails = person.PdsSectionIV.Q35b_Details,

                    Q35b_DateFiled = person.PdsSectionIV.Q35b_DateFiled,
                    Q35b_Status = person.PdsSectionIV.Q35b_Status,

                    // --- 36. Convicted of Crime ---
                    Q36_Convicted = person.PdsSectionIV.Q36_Convicted,
                    Q36_Details = person.PdsSectionIV.Q36_Details,

                    // --- 37. Separation from Service ---
                    Q37_Separated = person.PdsSectionIV.Q37_Separated,
                    Q37_Details = person.PdsSectionIV.Q37_Details,

                    // --- 38. Resigned for Political Campaign ---
                    Q38a_Candidate = person.PdsSectionIV.Q38a_Candidate,
                    Q38b_ResignedForCampaign = person.PdsSectionIV.Q38b_ResignedForCampaign,
                    Q38a_Details = person.PdsSectionIV.Q38a_Details,
                    Q38b_Details = person.PdsSectionIV.Q38b_Details,

                    // --- 39. Immigrant/Permanent Resident Status & Dual Citizenship ---
                    Q39_Country = person.PdsSectionIV.Q39_Country,
                    Q39_Details_Country = person.PdsSectionIV.Q39_Details_Country,

                    // --- 40. Other Information ---
                    Q40a_IndigenousGroup = person.PdsSectionIV.Q40a_IndigenousGroup,
                    Q40a_Details = person.PdsSectionIV.Q40a_Details,
                    Q40b_Disability = person.PdsSectionIV.Q40b_Disability,
                    Q40b_Details_IDNo = person.PdsSectionIV.Q40b_Details_IDNo,
                    Q40c_SoloParent = person.PdsSectionIV.Q40c_SoloParent,
                    Q40c_Details_IDNo = person.PdsSectionIV.Q40c_Details_IDNo

                } : null,

                // --- References ---
                References = person.PersonalInformationReferences.Select(r => new ReferenceDto
                {
                    ReferenceID = r.ReferenceID,
                    Name = r.Name,
                    OfficeOrResidentialAddress = r.OfficeOrResidentialAddress,
                    ContactNoOrEmail = r.ContactNoOrEmail
                }).ToList(),

                // --- Declaration (Section V) ---
                Declaration = person.Declaration != null ? new PdsSectionVDeclarationDto
                {
                    DeclarationID = person.Declaration.DeclarationID,
                    GovernmentIssuedID = person.Declaration.GovernmentIssuedID,
                    LicensePassportIDNo = person.Declaration.LicensePassportIDNo,
                    DateOfIssuance = person.Declaration.DateOfIssuance,
                    PlaceOfIssuance = person.Declaration.PlaceOfIssuance,
                    DateAccomplished = person.Declaration.DateAccomplished,

                    Signature = person.Declaration.SignaturePath,
                    AdminSignature = person.Declaration.AdminSignature,

                    PhotoFileKey = person.Declaration.PhotoPath,
                    PhotoOriginalName = person.Declaration.PhotoOriginalName,
                    RightThumbmarkFileKey = person.Declaration.RightThumbmarkPath,
                    DateSchedule = person.Declaration.DateSchedule
                } : null,
            };

            // 5. Call the PDF generation helper
            LogPdsAction("PDS_PDF_SUCCESS", $"Successfully generated PDF for PersonID: {id}.");
            // await _db.SaveChangesAsync(); // Uncomment if you need to save the log entry here

            // GeneratePdsPdf returns a FileContentResult
            return await GeneratePdsPdf(dto, genderMap, statusMap);
        }


        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- EMPLOYEE WORK SCHEDULE  ------------------------------------------
        // ---------------------------------------------------------------------------------------------------
        
        [HttpGet("my-work-schedule")]
        public async Task<IActionResult> GetMyWorkSchedule()
        {
            // 1. Get the EmployeeCode from the JWT
            var employeeIdFromToken = User.FindFirstValue("EmployeeCode");
            if (string.IsNullOrEmpty(employeeIdFromToken))
                return BadRequest("EmployeeCode missing in token.");

            // 2. Reuse your existing logic but filter by the specific ID
            return await FetchMySchedule(employeeIdFromToken.Trim());
        }
        private async Task<IActionResult> FetchMySchedule(string employeeId)
        {
            // 1. Get the specific Employee
            var employee = await _db.EnrollmentRegistries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID.Trim() == employeeId);

            if (employee == null) return NotFound(new { message = "Employee profile not found." });

            // 2. Get the active assignment for THIS employee only
            var assignment = await _db.EmployeeScheduleAssignments
                .AsNoTracking()
                .Include(a => a.Template)
                    .ThenInclude(t => t.Days)
                        .ThenInclude(d => d.ShiftSegments)
                .Where(a => a.IsActive && a.EmployeeID.Trim() == employeeId)
                .FirstOrDefaultAsync();

            // 3. Map to the same DTO you use for the HR view
            var response = new DisplayScheduleDto
            {
                EmployeeID = employee.EmployeeID.Trim(),
                FullName = $"{employee.FirstName.Trim()} {employee.LastName.Trim()}",
                Department = employee.DepartmentName,
                EffectiveDate = assignment?.EffectiveDate ?? DateTime.MinValue,
                WeeklySchedule = Enumerable.Range(0, 7).Select(dayVal =>
                {
                    var daySched = assignment?.Template?.Days.FirstOrDefault(d => d.DayOfWeek == dayVal);

                    return new DayScheduleView
                    {
                        DayName = ((DayOfWeek)dayVal).ToString(),
                        IsRestDay = daySched?.IsRestDay ?? true,
                        Shifts = (daySched == null || daySched.IsRestDay)
                            ? new List<ShiftView>()
                            : daySched.ShiftSegments.OrderBy(s => s.StartTime).Select(s => new ShiftView
                            {
                                ShiftName = s.ShiftType,
                                // Using the same formatting logic we established
                                TimeRange = $"{DateTime.Today.Add(s.StartTime):hh:mm tt} - {DateTime.Today.Add(s.EndTime):hh:mm tt}"
                            }).ToList()
                    };
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Fetches the attendance status and punch times for the current calendar day.
        /// </summary>
        [HttpGet("my-attendance-today")]
        public async Task<IActionResult> GetMyAttendanceToday()
        {
            // 1. Extract the EmployeeCode/ID from the JWT Token
            // Note: Using "EmployeeCode" as per your User.FindFirstValue change
            var employeeIdFromToken = User.FindFirstValue("EmployeeCode");
            if (string.IsNullOrEmpty(employeeIdFromToken))
                return BadRequest("EmployeeCode missing in token. Please re-login.");

            var today = DateTime.Today;

            // 2. The Bridge: Find the employee in the registry
            // We use .Trim() here because biometric IDs often have trailing spaces in DBs
            var employee = await _db.EnrollmentRegistries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID.Trim() == employeeIdFromToken.Trim());

            if (employee == null)
                return NotFound($"Employee profile not found for ID: {employeeIdFromToken}");

            // 3. Get the Attendance using the sanitized EmployeeID
            var log = await _db.AttendanceLogs_Processed
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.EmployeeID.Trim() == employee.EmployeeID.Trim() &&
                    l.WorkDate.Date == today);

            if (log == null)
            {
                return Ok(new
                {
                    hasLog = false,
                    message = "You haven't clocked in yet today.",
                    status = "Absent/Pending",
                    employeeId = employee.EmployeeID
                });
            }

            // 4. Return the formatted result
            return Ok(new
            {
                WorkDate = log.WorkDate.ToString("yyyy-MM-dd"),
                TimeIn = log.MorningIn?.ToString("HH:mm:ss"),
                Break = log.BreakIn?.ToString("HH:mm:ss"),
                TimeOut = log.AfternoonOut?.ToString("HH:mm:ss"),
                status = log.AttendanceStatus
            });
        }

        [HttpGet("my-attendance-history")]
        public async Task<IActionResult> GetMyAttendanceHistory(
         [FromQuery] DateTime? startDate,
         [FromQuery] DateTime? endDate,
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 20)
        {
            var employeeIdFromToken = User.FindFirstValue("EmployeeCode");
            if (string.IsNullOrEmpty(employeeIdFromToken))
                return BadRequest("EmployeeCode missing in token.");

            var query = _db.AttendanceLogs_Processed
                .AsNoTracking()
                .Where(l => l.EmployeeID.Trim() == employeeIdFromToken.Trim());

            if (startDate.HasValue)
                query = query.Where(l => l.WorkDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(l => l.WorkDate.Date <= endDate.Value.Date);
            else if (startDate.HasValue)
                query = query.Where(l => l.WorkDate.Date == startDate.Value.Date);

            var totalCount = await query.CountAsync();

            // 1. Fetch the raw data first (No projection yet)
            var rawLogs = await query
                .OrderByDescending(l => l.WorkDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Now it's in memory

            // 2. Project into the anonymous object in-memory (Safe for ?. operator)
            var logs = rawLogs.Select(l => new
            {
                l.EmployeeID,
                WorkDate = l.WorkDate.ToString("yyyy-MM-dd"),
                TimeIn = l.MorningIn?.ToString("HH:mm:ss"),
                Break = l.BreakIn?.ToString("HH:mm:ss"),
                TimeOut = l.AfternoonOut?.ToString("HH:mm:ss")
            }).ToList();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }
        /// <summary>
        /// Performs a "Smart Clock" action based on the current time and employee schedule for WFH employees.
        /// </summary>
        //[HttpPost("attendance/remote-clockin")]
        //public async Task<IActionResult> ClockAction()
        //{
        //    // 1. Identify User
        //    var employeeId = User.FindFirstValue("EmployeeID");
        //    var now = DateTime.Now;
        //    var today = now.Date;
        //    var currentTime = now.TimeOfDay;

        //    // 2. Fetch Schedule for current Day
        //    var dayOfWeek = (int)today.DayOfWeek;
        //    var schedule = await _db.EmployeeWorkSchedules
        //        .FirstOrDefaultAsync(s => s.EmployeeID == employeeId && s.DayOfWeek == dayOfWeek);

        //    if (schedule == null || schedule.IsRestDay)
        //        return BadRequest(new { message = "No work scheduled for today (Rest Day)." });

        //    // 3. Find/Create Processed Log
        //    var log = await _db.AttendanceLogs_Processed
        //        .FirstOrDefaultAsync(l => l.EmployeeID == employeeId && l.WorkDate == today);

        //    if (log == null)
        //    {
        //        log = new AttendanceLogs_Processed
        //        {
        //            EmployeeID = employeeId,
        //            WorkDate = today,
        //            AttendanceStatus = "Present"
        //        };
        //        _db.AttendanceLogs_Processed.Add(log);
        //    }

        //    string actionTaken = "";

        //    // 4. Logic Gates for Split Shifts
        //    // MORNING SESSION
        //    if (log.MorningIn == null && currentTime < schedule.MorningEnd)
        //    {
        //        log.MorningIn = now;
        //        actionTaken = "Morning Clock-In";
        //    }
        //    else if (log.MorningIn != null && log.MorningOut == null && currentTime < schedule.AfternoonStart)
        //    {
        //        log.MorningOut = now;
        //        actionTaken = "Morning Clock-Out (Lunch)";
        //    }
        //    // AFTERNOON SESSION
        //    else if (log.AfternoonIn == null && currentTime < schedule.AfternoonEnd)
        //    {
        //        log.AfternoonIn = now;
        //        actionTaken = "Afternoon Clock-In";
        //    }
        //    else if (log.AfternoonIn != null && log.AfternoonOut == null && (schedule.OvertimeStart == null || currentTime < schedule.OvertimeStart))
        //    {
        //        log.AfternoonOut = now;
        //        actionTaken = "Afternoon Clock-Out";
        //    }
        //    // OVERTIME SESSION
        //    else if (schedule.OvertimeStart.HasValue)
        //    {
        //        if (log.OvertimeIn == null) { log.OvertimeIn = now; actionTaken = "Overtime In"; }
        //        else { log.OvertimeOut = now; actionTaken = "Overtime Out"; }
        //    }

        //    // 5. AUTO-CALCULATION OF HOURS
        //    CalculateHours(log);

        //    log.LastUpdated = DateTime.Now;
        //    await _db.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        message = actionTaken,
        //        time = now.ToString("hh:mm tt"),
        //        totalHours = log.TotalRegularHours,
        //        otHours = log.TotalOvertimeHours
        //    });
        //}



        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- HELPER METHODS  --------------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        public class ScheduleUpdateRequest
        {
            public DateTime Date { get; set; }
            public List<int> PersonIDs { get; set; }
        }

        public class ValidationRequest
        {
            public string EmployeeId { get; set; }
        }

        static IContainer LabelStyle(IContainer container)
        {
            return container
                .Border(0.5f)
                .Background(Colors.Grey.Lighten4)
                .Padding(5)
                .AlignLeft()
                .AlignTop();
        }

        // Styling for the white value cells (where the user data goes)
        static IContainer ValueStyle(IContainer container)
        {
            return container
                .Border(0.5f)
                .PaddingLeft(4)
                .AlignLeft()
                .AlignMiddle();
        }

        static IContainer NameLabelStyle(IContainer container) => container
            .Background(Colors.Grey.Lighten4)
            .Border(0)
            .BorderVertical(0.5f) // No borders for these specific labels
            .Padding(5)
            .AlignLeft()
            .AlignMiddle();



        private async Task<FileContentResult> GeneratePdsPdf(PersonalInformationDto pdsData, Dictionary<int, string> genderMap, Dictionary<int, string> statusMap)
        {
            byte[]? photoBytes = null;
            string? photoPath = pdsData.Declaration?.PhotoFileKey;

            if (!string.IsNullOrWhiteSpace(photoPath))
            {
                try
                {
                    string fullUrl = GetFullPhotoUrl(photoPath);

                    // Use IHttpClientFactory if available, or initialize directly
                    using (var httpClient = new HttpClient())
                    {
                        photoBytes = await httpClient.GetByteArrayAsync(fullUrl);
                    }
                }
                catch (Exception ex)
                {
                    //LogPdsAction("PDF_IMAGE_EMBED_ERROR", $"Failed to download photo: {ex.Message}");
                    photoBytes = null; // Ensure it's null if download failed
                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuration
                    page.Size(PageSizes.Legal);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Border(1).Column(header =>
                    {
                        header.Spacing(2);

                        // Form Revision (Top Left)
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CS Form No. 212").FontSize(8).Bold().Italic();
                                c.Item().Text("Revised 2017").FontSize(8).Italic();
                            });
                        });

                        // Main Title
                        header.Item().AlignCenter().Text("PERSONAL DATA SHEET")
                            .Black().ExtraBold().FontSize(20);

                        // Warning Text
                        header.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("WARNING: Any misrepresentation made in the Personal Data Sheet and the Work Experience Sheet shall cause the filing of administrative/criminal case/s against the person concerned.")
                                .FontSize(7).Bold().Italic();
                        });
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(2);

                        // -----------------------------------------------------------------------------------
                        // I. PERSONAL INFORMATION
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < 12; i++) columns.RelativeColumn();
                            });

                            // SECTION HEADER
                            table.Cell().ColumnSpan(12).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("I. PERSONAL INFORMATION").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // Row 2: SURNAME (Uses NameLabelStyle)
                            table.Cell().ColumnSpan(3).Element(NameLabelStyle).Text("2. SURNAME").FontSize(7);
                            table.Cell().ColumnSpan(9).Element(ValueStyle).Text(pdsData.Surname?.ToUpper() ?? "N/A").FontSize(9);

                            // Row: FIRST NAME + EXTENSION (Uses NameLabelStyle for the name part)
                            table.Cell().ColumnSpan(3).Element(NameLabelStyle).Text("    FIRST NAME").FontSize(7);
                            table.Cell().ColumnSpan(5).Element(ValueStyle).Text(pdsData.FirstName?.ToUpper() ?? "N/A").FontSize(9);
                            table.Cell().ColumnSpan(2).Element(LabelStyle).Text("NAME EXTENSION").FontSize(6);
                            table.Cell().ColumnSpan(2).Element(ValueStyle).Text(pdsData.NameExtension?.ToUpper() ?? "N/A").FontSize(9);

                            // Row: MIDDLE NAME (Uses NameLabelStyle)
                            table.Cell().ColumnSpan(3).Element(NameLabelStyle).Text("    MIDDLE NAME").FontSize(7);
                            table.Cell().ColumnSpan(9).Element(ValueStyle).Text(pdsData.MiddleName?.ToUpper() ?? "N/A").FontSize(9);

                            // --- SPLIT SECTION (Left side now perfectly aligns with Names/DOB labels) ---

                            // --- ROW 3 & 4 (Left) + ROW 16 (Right) ---
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("3. DATE OF BIRTH").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.DateOfBirth?.ToString("MM/dd/yyyy") ?? "");

                            // Citizenship (16) on the Right
                            table.Cell().RowSpan(3).ColumnSpan(3).Element(LabelStyle).Text("16. CITIZENSHIP").FontSize(7);
                            table.Cell().RowSpan(3).ColumnSpan(3).Element(ValueStyle).Column(c =>
                            {
                                c.Item().Text(pdsData.Citizenship ?? "");
                                c.Item().PaddingTop(2).Text("Pls. indicate country:").FontSize(5);
                            });

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("4. PLACE OF BIRTH").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.PlaceOfBirth ?? "");

                            // --- ROW 5 (Left) ---
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("5. SEX").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(genderMap.GetValueOrDefault(pdsData.SexID ?? 0, ""));

                            // --- ROW 6, 7, 8 (Left) + ROW 17 (Residential Address Right) ---
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("6. CIVIL STATUS").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(statusMap.GetValueOrDefault(pdsData.CivilStatusID ?? 0, ""));

                            // RESIDENTIAL ADDRESS (17) on the Right
                            table.Cell().RowSpan(3).ColumnSpan(3).Element(LabelStyle).Text("17. RESIDENTIAL ADDRESS").FontSize(7);
                            table.Cell().RowSpan(3).ColumnSpan(3).Element(ValueStyle).Column(c =>
                            {
                                // Concatenate or display fields. 
                                // Using a vertical stack for readability
                                c.Item().Text($"{pdsData.ResHouseBlockLot ?? ""} {pdsData.ResStreet ?? ""}, {pdsData.ResSubdivisionVillage ?? ""}").FontSize(7);
                                c.Item().Text($"{pdsData.ResCityMunicipality ?? ""}, {pdsData.ResProvince ?? ""}").FontSize(7);
                                c.Item().PaddingTop(4).Row(r => {
                                    r.RelativeItem().BorderTop(0.5f).Text("ZIP CODE").FontSize(5).Italic();
                                    r.AutoItem().BorderTop(0.5f).Text(pdsData.ResZip ?? "").FontSize(7).Bold();
                                });
                            });

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("7. HEIGHT (m)").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.HeightCM?.ToString() ?? "");

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("8. WEIGHT (kg)").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.WeightKG?.ToString() ?? "");

                            // --- ROW 9, 10, 11, 12 (Left) + ROW 18 (Permanent Address Right) ---
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("9. BLOOD TYPE").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.BloodType ?? "");

                            // --- PERMANENT ADDRESS (18) ---
                            table.Cell().RowSpan(4).ColumnSpan(3).Element(LabelStyle).Text("18. PERMANENT ADDRESS").FontSize(7);
                            table.Cell().RowSpan(4).ColumnSpan(3).Element(ValueStyle).Column(c =>
                            {
                                c.Item().Text($"{pdsData.PermHouseBlockLot ?? ""} {pdsData.PermStreet ?? ""}, {pdsData.PermSubdivisionVillage ?? ""}").FontSize(7);
                                c.Item().Text($"{pdsData.PermCityMunicipality ?? ""}, {pdsData.PermProvince ?? ""}").FontSize(7);
                                c.Item().PaddingTop(4).Row(r => {
                                    r.RelativeItem().BorderTop(0.5f).Text("ZIP CODE").FontSize(5).Italic();
                                    r.AutoItem().BorderTop(0.5f).Text(pdsData.PermZip ?? "").FontSize(7).Bold();
                                });
                            });

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("10. GSIS ID NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.GsisID ?? "");

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("11. PAG-IBIG ID NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.PagibigID ?? "");

                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("12. PHILHEALTH NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.PhilhealthID ?? "");

                            // --- BOTTOM ROWS (Swapping Contact info to Right) ---

                            // Row 13 (Left) + Row 19 (Right)
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("13. SSS NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.SssNo ?? "");
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("19. TELEPHONE NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.TelephoneNo ?? "");

                            // Row 14 (Left) + Row 20 (Right)
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("14. TIN NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.Tin ?? "");
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("20. MOBILE NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.MobileNo ?? "");

                            // Row 15 (Left) + Row 21 (Right)
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("15. AGENCY EMPLOYEE NO.").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.AgencyEmployeeNo ?? "");
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("21. E-MAIL ADDRESS (if any)").FontSize(7);
                            table.Cell().ColumnSpan(3).Element(ValueStyle).Text(pdsData.Email ?? "");
                        });



                        // -----------------------------------------------------------------------------------
                        // II. FAMILY BACKGROUND (Complete CSC PDS Layout)
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                // 12 columns total: Left Side (7) + Right Side (5)
                                for (int i = 0; i < 12; i++) columns.RelativeColumn();
                            });

                            // SECTION HEADER
                            table.Cell().ColumnSpan(12).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("II. FAMILY BACKGROUND").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // --- ROW 1: HEADERS ---
                            table.Cell().ColumnSpan(3).Element(LabelStyle).Text("22. SPOUSE'S SURNAME").FontSize(7);
                            table.Cell().ColumnSpan(4).Element(ValueStyle).Text(pdsData.SpouseSurname?.ToUpper() ?? "").FontSize(8);

                            table.Cell().ColumnSpan(4).Element(LabelStyle).AlignCenter().Text("23. NAME of CHILDREN (Write full name and list all)").FontSize(6.5f);
                            table.Cell().ColumnSpan(1).Element(LabelStyle).AlignCenter().Text("DATE OF BIRTH (mm/dd/yyyy)").FontSize(5.5f);

                            var children = pdsData.Children ?? new List<ChildDto>();

                            // Standard 13 rows to cover Mother's Middle Name (index 12)
                            for (int i = 0; i < 13; i++)
                            {
                                var child = children.ElementAtOrDefault(i);

                                // --- LEFT SIDE: SPOUSE / FATHER / MOTHER (7 Columns Total) ---
                                if (i == 0 || i == 7 || i == 11)
                                {
                                    table.Cell().ColumnSpan(3).Element(LabelStyle).Text("    FIRST NAME").FontSize(7);

                                    table.Cell().ColumnSpan(4).Element(ValueStyle).Row(r => {
                                        string firstName = (i == 0) ? pdsData.SpouseFirstName : (i == 7 ? pdsData.FatherFirstName : pdsData.MotherFirstName);
                                        string ext = (i == 0) ? pdsData.SpouseNameExtension : (i == 7 ? pdsData.FatherNameExtension : pdsData.MotherNameExtension);

                                        r.RelativeItem().Text(firstName?.ToUpper() ?? "").FontSize(8);

                                        r.AutoItem().BorderLeft(0.5f).Background(Colors.Grey.Lighten4).PaddingLeft(2).Width(55).Column(c => {
                                            c.Item().Text("NAME EXTENSION (JR., SR.)").FontSize(4);
                                            c.Item().Text(ext ?? "N/A").FontSize(7);
                                        });
                                    });
                                }
                                else
                                {
                                    string labelText = "";
                                    string valueText = "";

                                    switch (i)
                                    {
                                        case 1: labelText = "    MIDDLE NAME"; valueText = pdsData.SpouseMiddleName; break;
                                        case 2: labelText = "    OCCUPATION"; valueText = pdsData.SpouseOccupation; break;
                                        case 3: labelText = "    EMPLOYER/BUSINESS NAME"; valueText = pdsData.SpouseEmployer; break;
                                        case 4: labelText = "    BUSINESS ADDRESS"; valueText = pdsData.SpouseBusinessAddress; break;
                                        case 5: labelText = "    TELEPHONE NO."; valueText = pdsData.SpouseTelephone; break;
                                        case 6: labelText = "24. FATHER'S SURNAME"; valueText = pdsData.FatherSurname; break;
                                        case 8: labelText = "    MIDDLE NAME"; valueText = pdsData.FatherMiddleName; break;
                                        case 9: labelText = "25. MOTHER'S MAIDEN NAME"; valueText = ""; break;
                                        case 10: labelText = "    SURNAME"; valueText = pdsData.MotherSurname; break;
                                        case 12: labelText = "    MIDDLE NAME"; valueText = pdsData.MotherMiddleName; break;
                                        default: labelText = ""; valueText = ""; break;
                                    }

                                    table.Cell().ColumnSpan(3).Element(LabelStyle).Text(labelText).FontSize(7);
                                    table.Cell().ColumnSpan(4).Element(ValueStyle).Text(valueText?.ToUpper() ?? "").FontSize(8);
                                }

                                // --- RIGHT SIDE: CHILDREN DATA OR FOOTER (5 Columns Total) ---
                                // If we are on the last row (Mother's Middle Name row), we check if there's a child. 
                                // If not, we place the red footer text here.
                                if (i == 12 && child == null)
                                {
                                    table.Cell().ColumnSpan(5).Element(ValueStyle).AlignCenter()
                                        .Text("(Continue on separate sheet if necessary)").FontColor(Colors.Red.Medium).Italic().FontSize(6);
                                }
                                else
                                {
                                    table.Cell().ColumnSpan(4).Element(ValueStyle).Text(child?.FullName?.ToUpper() ?? "").FontSize(7);
                                    table.Cell().ColumnSpan(1).Element(ValueStyle).AlignCenter().Text(child?.DateOfBirth?.ToString("MM/dd/yyyy") ?? "").FontSize(7);
                                }
                            }
                        });

                        // -----------------------------------------------------------------------------------
                        // III. EDUCATIONAL BACKGROUND (9-Column Grid)
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                // 9 distinct columns to match the official PDS grid
                                columns.RelativeColumn(2.0f); // 1: Level
                                columns.RelativeColumn(3.0f); // 2: Name of School
                                columns.RelativeColumn(2.0f); // 3: Degree/Course
                                columns.RelativeColumn(0.8f); // 4: Period From
                                columns.RelativeColumn(0.8f); // 5: Period To
                                columns.RelativeColumn(1.2f); // 6: Highest Level/Units
                                columns.RelativeColumn(1.0f); // 7: Year Graduated
                                columns.RelativeColumn(1.2f); // 8: Scholarship/Honors
                            });

                            // SECTION HEADER (Spans all 8 columns)
                            table.Cell().ColumnSpan(8).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("III. EDUCATIONAL BACKGROUND").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // --- ROW 1: MAIN HEADERS ---
                            // We use RowSpan(2) for labels and ColumnSpan(2) for the Period group
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().AlignMiddle().Text("26. LEVEL").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().AlignMiddle().Text("NAME OF SCHOOL\n(Write in full)").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().AlignMiddle().Text("BASIC EDUCATION/DEGREE/COURSE\n(Write in full)").FontSize(7);

                            // Spans the specific From and To columns
                            table.Cell().ColumnSpan(2).Element(LabelStyle).AlignCenter().Text("PERIOD OF ATTENDANCE").FontSize(7);

                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("HIGHEST LEVEL/\nUNITS EARNED\n(if not graduated)").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("YEAR GRADUATED").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("SCHOLARSHIP/\nACADEMIC HONORS\nRECEIVED").FontSize(7);

                            // --- ROW 2: THE SEPARATED PERIOD SUB-HEADERS ---
                            // These cells only appear under the ColumnSpan(2) area
                            table.Cell().Element(LabelStyle).AlignCenter().Text("From").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("To").FontSize(7);

                            // --- DATA ROWS ---
                            var levels = new[] { "ELEMENTARY", "SECONDARY", "VOCATIONAL / TRADE COURSE", "COLLEGE", "GRADUATE STUDIES" };

                            foreach (var level in levels)
                            {
                                var edu = pdsData.EducationRecords?.FirstOrDefault(e => e.Level?.ToUpper() == level) ?? new EducationDto();

                                table.Cell().Element(LabelStyle).PaddingLeft(2).Text(level).FontSize(7);
                                table.Cell().Element(ValueStyle).Text(edu.SchoolName?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).Text(edu.Degree?.ToUpper() ?? "").FontSize(7);

                                // NOW SEPARATE CELLS
                                table.Cell().Element(ValueStyle).AlignCenter().Text(edu.AttendanceFrom).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(edu.AttendanceTo).FontSize(7);

                                table.Cell().Element(ValueStyle).AlignCenter().Text(edu.HighestLevel?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(edu.YearGraduated?.ToString() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).Text(edu.Honors?.ToUpper() ?? "").FontSize(7);
                            }

                            // --- FOOTER RED TEXT ---
                            table.Cell().ColumnSpan(8).Element(ValueStyle).AlignCenter()
                                .Text("(Continue on separate sheet if necessary)").FontColor(Colors.Red.Medium).Italic().FontSize(6);
                        });

                        // -----------------------------------------------------------------------------------
                        // SIGNATURE AND DATE FOOTER (Separate table for stability)
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); // SIGNATURE label
                                columns.RelativeColumn(4.5f); // Space for manual signing
                                columns.RelativeColumn(1.0f); // DATE label
                                columns.RelativeColumn(2.0f); // Date value
                            });

                            table.Cell().Element(LabelStyle).Padding(4).AlignCenter().Text("SIGNATURE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).Padding(4).Text("");

                            table.Cell().Element(LabelStyle).Padding(4).AlignCenter().Text("DATE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).Padding(4).AlignCenter().Text(DateTime.Now.ToString("MM/dd/yyyy")).FontSize(8.5f);
                        });

                        // ===================================================================================
                        // 🚀 PAGE BREAK HERE (Section IV starts on Page 2)
                        // ===================================================================================
                        column.Item().PageBreak();


                        // -----------------------------------------------------------------------------------
                        // IV. CIVIL SERVICE ELIGIBILITY
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                            });

                            table.Cell().ColumnSpan(6).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("IV. CIVIL SERVICE ELIGIBILITY").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // Headers with vertical padding for more height
                            table.Cell().RowSpan(2).Element(LabelStyle).PaddingVertical(5).AlignCenter().AlignMiddle().Text("27. CAREER SERVICE...").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).PaddingVertical(5).AlignCenter().AlignMiddle().Text("RATING").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).PaddingVertical(5).AlignCenter().AlignMiddle().Text("DATE OF EXAM").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).PaddingVertical(5).AlignCenter().AlignMiddle().Text("PLACE OF EXAM").FontSize(7);
                            table.Cell().ColumnSpan(2).Element(LabelStyle).PaddingVertical(2).AlignCenter().AlignMiddle().Text("LICENSE (if applicable)").FontSize(7);

                            table.Cell().Element(LabelStyle).AlignCenter().Text("NUMBER").FontSize(6);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("VALIDITY").FontSize(5.5f);

                            var eligibilityRecords = pdsData.CivilServiceEligibilities ?? new List<CivilServiceEligibilityDTO>();
                            for (int i = 0; i < 7; i++)
                            {
                                var record = eligibilityRecords.ElementAtOrDefault(i) ?? new CivilServiceEligibilityDTO();

                                // .MinHeight(20) ensures the rows are taller even if empty
                                table.Cell().Element(ValueStyle).MinHeight(20).Text(record.CareerService?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.Rating).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.DateOfExamination).FontSize(7);
                                table.Cell().Element(ValueStyle).Text(record.PlaceOfExamination?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.LicenseNumber).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.LicenseValidity).FontSize(7);
                            }

                            table.Cell().ColumnSpan(6).Element(ValueStyle).AlignCenter()
                                .Text("(Continue on separate sheet if necessary)").FontColor(Colors.Red.Medium).Italic().FontSize(6);
                        });

                        // -----------------------------------------------------------------------------------
                        // V. WORK EXPERIENCE
                        // -----------------------------------------------------------------------------------
                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.8f); columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(3); columns.RelativeColumn(3);
                                columns.RelativeColumn(1); columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f); columns.RelativeColumn(0.6f);
                            });

                            table.Cell().ColumnSpan(8).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("V. WORK EXPERIENCE...").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // Main Headers with MinHeight for better visibility
                            table.Cell().ColumnSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("28. INCLUSIVE DATES").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("POSITION TITLE").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("DEPARTMENT / AGENCY").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("MONTHLY SALARY").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("SALARY GRADE").FontSize(6);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("STATUS").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).MinHeight(25).AlignCenter().Text("GOV'T SERVICE").FontSize(6);

                            table.Cell().Element(LabelStyle).AlignCenter().Text("From").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("To").FontSize(7);

                            var workRecords = pdsData.WorkExperiences ?? new List<WorkExperienceDTO>();
                            for (int i = 0; i < 28; i++)
                            {
                                var work = workRecords.ElementAtOrDefault(i) ?? new WorkExperienceDTO();

                                // MinHeight(18) makes these rows significantly taller
                                table.Cell().Element(ValueStyle).MinHeight(18).AlignCenter().Text(work.DateFrom).FontSize(7);
                                table.Cell().Element(ValueStyle).MinHeight(18).AlignCenter().Text(work.DateTo).FontSize(7);
                                table.Cell().Element(ValueStyle).Text(work.PositionTitle?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).Text(work.DepartmentAgencyCompany?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(work.MonthlySalary).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(work.SalaryGradeStep ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(work.StatusOfAppointment ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(work.IsGovernmentService ? "" : "").FontSize(7);
                            }

                            table.Cell().ColumnSpan(8).Element(ValueStyle).AlignCenter()
                                .Text("(Continue on separate sheet if necessary)").FontColor(Colors.Red.Medium).Italic().FontSize(6);
                        });

                        // SIGNATURE FOOTER (Increased height for the signing area)
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); columns.RelativeColumn(4.5f);
                                columns.RelativeColumn(1.0f); columns.RelativeColumn(2.0f);
                            });
                            table.Cell().Element(LabelStyle).Padding(10).AlignCenter().Text("SIGNATURE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).MinHeight(30).Text(""); // More space for actual signature
                            table.Cell().Element(LabelStyle).Padding(10).AlignCenter().Text("DATE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).Padding(10).AlignCenter().Text(DateTime.Now.ToString("MM/dd/yyyy")).FontSize(8.5f);
                        });


                        // ===================================================================================
                        // 🚀 PAGE BREAK HERE (Section IV starts on Page 2)
                        // ===================================================================================
                        column.Item().PageBreak();

                        // -----------------------------------------------------------------------------------
                        // VI. VOLUNTARY WORK (Condensed)
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f); columns.RelativeColumn(1); columns.RelativeColumn(3);
                            });

                            table.Cell().ColumnSpan(5).Background(Colors.Grey.Medium).Border(1).Padding(1)
                                .Text("VI. VOLUNTARY WORK...").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            // Headers
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("29. NAME & ADDRESS").FontSize(7);
                            table.Cell().ColumnSpan(2).Element(LabelStyle).AlignCenter().Text("INCLUSIVE DATES").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("HOURS").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("POSITION").FontSize(7);

                            table.Cell().Element(LabelStyle).AlignCenter().Text("From").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("To").FontSize(7);

                            var voluntaryRecords = pdsData.VoluntaryWorks ?? new List<VoluntaryWorkDTO>();
                            for (int i = 0; i < 7; i++)
                            {
                                var record = voluntaryRecords.ElementAtOrDefault(i) ?? new VoluntaryWorkDTO();
                                // Reduced MinHeight from 20 to 14
                                table.Cell().Element(ValueStyle).MinHeight(14).Text(record.Organization?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.DateFrom).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.DateTo).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.NumberOfHours).FontSize(7);
                                table.Cell().Element(ValueStyle).Text(record.Position?.ToUpper() ?? "").FontSize(7);
                            }
                        });

                        // -----------------------------------------------------------------------------------
                        // VII. LEARNING AND DEVELOPMENT (Condensed)
                        // -----------------------------------------------------------------------------------
                        // Reduced PaddingTop from 5 to 2
                        column.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); columns.RelativeColumn(0.8f); columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1); columns.RelativeColumn(1.2f); columns.RelativeColumn(2.5f);
                            });

                            table.Cell().ColumnSpan(6).Background(Colors.Grey.Medium).Border(1).Padding(1)
                                .Text("VII. LEARNING AND DEVELOPMENT...").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("30. TITLE OF TRAINING").FontSize(7);
                            table.Cell().ColumnSpan(2).Element(LabelStyle).AlignCenter().Text("DATES").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("HOURS").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("TYPE").FontSize(7);
                            table.Cell().RowSpan(2).Element(LabelStyle).AlignCenter().Text("CONDUCTED BY").FontSize(7);

                            table.Cell().Element(LabelStyle).AlignCenter().Text("From").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("To").FontSize(7);

                            var trainingRecords = pdsData.Trainings ?? new List<TrainingDTO>();
                            for (int i = 0; i < 21; i++)
                            {
                                var record = trainingRecords.ElementAtOrDefault(i) ?? new TrainingDTO();
                                // Reduced MinHeight from 18 to 13
                                table.Cell().Element(ValueStyle).MinHeight(13).Text(record.Title?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.DateFrom).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.DateTo).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.NumberOfHours).FontSize(7);
                                table.Cell().Element(ValueStyle).AlignCenter().Text(record.TypeOfLD?.ToUpper() ?? "").FontSize(7);
                                table.Cell().Element(ValueStyle).Text(record.ConductedBy?.ToUpper() ?? "").FontSize(7);
                            }
                        });

                        // -----------------------------------------------------------------------------------
                        // VIII. OTHER INFORMATION (Condensed)
                        // -----------------------------------------------------------------------------------
                        column.Item().PaddingTop(2).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); columns.RelativeColumn(2); columns.RelativeColumn(2);
                            });

                            table.Cell().ColumnSpan(3).Background(Colors.Grey.Medium).Border(1).Padding(1)
                                .Text("VIII. OTHER INFORMATION").Bold().FontSize(10).FontColor(Colors.White).Italic();

                            table.Cell().Element(LabelStyle).AlignCenter().Text("31. SKILLS/HOBBIES").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("32. DISTINCTIONS").FontSize(7);
                            table.Cell().Element(LabelStyle).AlignCenter().Text("33. MEMBERSHIP").FontSize(7);

                            var skillRecords = pdsData.SkillsHobbies ?? new List<SkillHobbyDTO>();
                            var distRecords = pdsData.Distinctions ?? new List<DistinctionDTO>();
                            var membRecords = pdsData.Memberships ?? new List<MembershipDTO>();
                            for (int i = 0; i < 7; i++)
                            {
                                // Reduced MinHeight from 20 to 14
                                table.Cell().Element(ValueStyle).MinHeight(14).Text(pdsData.SkillsHobbies != null ? pdsData.SkillsHobbies.ElementAtOrDefault(i) : "").FontSize(7);
                                table.Cell().Element(ValueStyle).MinHeight(14).Text(pdsData.Distinctions != null ? pdsData.Distinctions.ElementAtOrDefault(i) : "").FontSize(7);
                                table.Cell().Element(ValueStyle).MinHeight(14).Text(pdsData.Memberships != null ? pdsData.Memberships.ElementAtOrDefault(i): "").FontSize(7);
                            }
                        });

                        // Footer (Condensed)
                        column.Item().PaddingTop(2).Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.RelativeColumn(1.5f); columns.RelativeColumn(4.5f); columns.RelativeColumn(1); columns.RelativeColumn(2); });
                            // Reduced Padding from 8 to 4
                            table.Cell().Element(LabelStyle).Padding(4).AlignCenter().Text("SIGNATURE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).MinHeight(20).Text("");
                            table.Cell().Element(LabelStyle).Padding(4).AlignCenter().Text("DATE").Bold().FontSize(8.5f);
                            table.Cell().Element(ValueStyle).Padding(4).AlignCenter().Text(DateTime.Now.ToString("MM/dd/yyyy")).FontSize(8.5f);
                        });

                        // ===================================================================================
                        // 🚀 PAGE BREAK HERE (Section IV starts on Page 2)
                        // ===================================================================================
                        column.Item().PageBreak();

                        // -----------------------------------------------------------------------------------
                        // IV. OTHER INFORMATION (Questions 34-40) - Official CSC Text
                        // -----------------------------------------------------------------------------------
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5.5f); // Question
                                columns.RelativeColumn(1);    // Yes/No
                                columns.RelativeColumn(3.5f); // Details
                            });

                            // Section Header
                            table.Cell().ColumnSpan(3).Background(Colors.Grey.Medium).Border(1).Padding(2)
                                .Text("34. - 40. OTHER INFORMATION").Bold().FontSize(9).FontColor(Colors.White).Italic();

                            var s4 = pdsData.PdsSectionIV ?? new PdsSectionIVDto();

                            // Helper for consistency
                            Action<string, bool?, string> AddQuestionRow = (question, yesNo, details) =>
                            {
                                table.Cell().Border(1).Padding(3).MinHeight(25).Text(question).FontSize(7f);
                                table.Cell().Border(1).AlignCenter().AlignMiddle().Text(yesNo.HasValue ? (yesNo.Value ? "YES" : "NO") : "").FontSize(8).Bold();
                                table.Cell().Border(1).Padding(3).Text(details ?? "").FontSize(7f);
                            };

                            // --- Q34 ---
                            table.Cell().Border(1).Padding(3).Text("34. a. Are you related within the third degree of consanguinity or affinity to the appointing authority, chief of office, or to the person who has immediate supervision over you in the Office, Bureau or Department where you will be apppointed?").FontSize(7f);
                            table.Cell().Border(1).AlignCenter().AlignMiddle().Text(s4.Q34a_RelatedWithin3rd == true ? "YES" : "NO").FontSize(8).Bold();
                            table.Cell().RowSpan(2).Border(1).Padding(3).Text($"If YES, give details:\n{s4.Q34_Details}").FontSize(7f);

                            table.Cell().Border(1).Padding(3).Text("    b. Are you related within the fourth degree of consanguinity or affinity to the appointing authority, chief of office, or to the person who has immediate supervision over you in the Office, Bureau or Department where you will be apppointed?\n(for Local Government Unit)").FontSize(7f);
                            table.Cell().Border(1).AlignCenter().AlignMiddle().Text(s4.Q34b_RelatedWithin4th == true ? "YES" : "NO").FontSize(8).Bold();

                            // --- Q35 ---
                            AddQuestionRow("35. a. Have you ever been found guilty of any administrative offense?", s4.Q35a_AdminOffense, $"If YES, give details:\n{s4.Q35a_AdminDetails}");
                            AddQuestionRow("    b. Have you been criminally charged before any court, tribunal or quasi-judicial body, or be a respondent in any administrative case?", s4.Q35b_CriminallyCharged, $"If YES, give details:\nDate Filed: {s4.Q35b_DateFiled}\nStatus: {s4.Q35b_CriminalDetails}");

                            // --- Q36 ---
                            AddQuestionRow("36. Have you ever been convicted of any crime or violation of any law, decree, ordinance or regulation by any court or tribunal?", s4.Q36_Convicted, $"If YES, give details:\n{s4.Q36_Details}");

                            // --- Q37 ---
                            AddQuestionRow("37. Have you ever been separated from the service in any of the following modes: resignation, retirement, dropped from the rolls, dismissal, termination, end of term, finished contract or phased out (abolition) in the public or private sector?", s4.Q37_Separated, $"If YES, give details:\n{s4.Q37_Details}");

                            // --- Q38 ---
                            AddQuestionRow("38. a. Have you ever been a candidate in a national or local election held within the last year (except Barangay election)?", s4.Q38a_Candidate, $"If YES, give details:\n{s4.Q38a_Details}");
                            AddQuestionRow("    b. Have you resigned from the government service during the three (3)-month period before the last election to promote/force any candidacy?", s4.Q38b_ResignedForCampaign, $"If YES, give details:\n{s4.Q38b_Details}");

                            // --- Q39 ---
                            AddQuestionRow("39. Have you acquired the status of an immigrant or permanent resident of another country?", s4.Q39_Country, $"If YES, give details (country):\n{s4.Q39_Details_Country}");

                            // --- Q40 ---
                            AddQuestionRow("40. a. Are you a member of any indigenous group?", s4.Q40a_IndigenousGroup, $"If YES, please specify:\n{s4.Q40a_Details}");
                            AddQuestionRow("    b. Are you a person with disability?", s4.Q40b_Disability, $"If YES, please specify ID No:\n{s4.Q40b_Details_IDNo}");
                            AddQuestionRow("    c. Are you a solo parent?", s4.Q40c_SoloParent, $"If YES, please specify ID No:\n{s4.Q40c_Details_IDNo}");
                        });

                        column.Item().PaddingTop(5).Table(mainTable =>
                        {
                            // 1. Define global 4-column grid
                            mainTable.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3); // Col 1
                                cols.RelativeColumn(3); // Col 2
                                cols.RelativeColumn(2); // Col 3
                                cols.RelativeColumn(2); // Col 4: Passport/Thumbmark
                            });

                            // -----------------------------------------------------------------------------------
                            // ROW 1: REFERENCES (41)
                            // -----------------------------------------------------------------------------------
                            // Span the first 3 columns with the references table
                            mainTable.Cell().ColumnSpan(3).Table(refTable =>
                            {
                                refTable.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); });

                                refTable.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten2).Border(1).Padding(2)
                                    .Text("41. REFERENCES").Bold().FontSize(7);

                                refTable.Cell().Border(1).AlignCenter().Text("NAME").FontSize(6f).SemiBold();
                                refTable.Cell().Border(1).AlignCenter().Text("ADDRESS").FontSize(6f).SemiBold();
                                refTable.Cell().Border(1).AlignCenter().Text("CONTACT").FontSize(6f).SemiBold();

                                var refs = pdsData.References ?? new List<ReferenceDto>();
                                for (int i = 0; i < 3; i++)
                                {
                                    var r = refs.ElementAtOrDefault(i) ?? new ReferenceDto();
                                    refTable.Cell().Border(1).Height(20).PaddingLeft(4).AlignMiddle().Text(r.Name?.ToUpper() ?? "").FontSize(7);
                                    refTable.Cell().Border(1).PaddingLeft(4).AlignMiddle().Text(r.OfficeOrResidentialAddress?.ToUpper() ?? "").FontSize(7);
                                    refTable.Cell().Border(1).PaddingLeft(4).AlignMiddle().Text(r.ContactNoOrEmail?.ToUpper() ?? "").FontSize(7);
                                }
                            });

                            // Col 4: The Photo Box header
                            mainTable.Cell().Border(1).Background(Colors.Grey.Lighten2).AlignCenter().AlignMiddle()
                                .Text("PHOTO").FontSize(6f).Bold();
                            
                            // -----------------------------------------------------------------------------------
                            // ROW 2: DECLARATION & OATH (42)
                            // -----------------------------------------------------------------------------------
                            // Spans the first 3 columns
                            mainTable.Cell().ColumnSpan(3).Border(1).Padding(5).Column(c => {
                                c.Item().Text("42. I declare under oath that I have personally accomplished this Personal Data Sheet...").FontSize(7f);
                            });

                            // Col 4: The Photo & Thumbmark
                            mainTable.Cell().Border(1).Column(col => {
                                col.Item().Height(100).AlignCenter().AlignMiddle().Element(e => {
                                    if (photoBytes != null) e.Image(photoBytes).FitArea();
                                    else e.Text("Passport-sized picture\n4.5 cm X 3.5 cm").FontSize(6).AlignCenter();
                                });
                                col.Item().BorderTop(1).Height(20).AlignCenter().AlignMiddle().Text("Right Thumbmark").FontSize(7);
                            });
                        });
                    }
                   );

                    // Footer (optional)
                    page.Footer()
                        .AlignCenter()
                        .Text("For Development");
                });
            });

            // Generate the PDF bytes
            var pdfBytes = document.GeneratePdf();

            // Return the file as a downloadable result
            return new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"PDS_{pdsData.Surname}_{pdsData.FirstName}_{pdsData.PersonID}.pdf"
            };
        }

        private string GetFullPhotoUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            // 1. Get the current HttpContext to build the base URL
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                // For typical Web API requests, throwing an exception is safer:
                throw new InvalidOperationException("Cannot resolve base URL outside of an HTTP request context.");
            }

            // 2. Build the full URL: Scheme (http/https) + Host (domain) + Path (from DB)
            var baseUri = new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? -1);

            // Combine the base URL with the relative path from the database.
            // Ensure the relative path starts with a '/', or it's handled correctly by Uri.
            string absolutePath = relativePath.TrimStart('~');

            return baseUri.Uri.GetLeftPart(UriPartial.Authority) + absolutePath;
        }

        public class MyScheduleRequest
        {
            [Required]
            public DateTime PreferredDate { get; set; }
        }

    }
}

