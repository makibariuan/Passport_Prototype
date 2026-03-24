using FastMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using Org.BouncyCastle.Asn1.IsisMtt.Ocsp;
using SeniorCitizen.Server.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Data;
using System.Security.Claims;
using ZXing;
using ZXing.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static OnlineRegistration.Server.Controllers.EmployeeController;

namespace OnlineRegistration.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    //[Authorize]
    public class HRController : ControllerBase
    {
        private readonly EmployeesDbContext _db;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailQueue _emailQueue;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPgpService _pgpService;
        private readonly string _connectionString;

        public HRController(EmployeesDbContext db, IConfiguration config, IEmailQueue emailQueue, IFileService fileService, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor, AppDbContext context, IPgpService pgpService)
        {
            _db = db;
            _config = config;
            _emailQueue = emailQueue;
            _fileService = fileService;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _pgpService = pgpService;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }
        

        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- HR EMPLOYEE DASHBOARD --------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        //authoization policy: RequireAdministrativeView: UserRole must be 1(SuperAdmin), 2(SystemUser), 6(HR)

        //Status 0: Pending Schedule
        //Status 1: Scheduled
        //Status 2: Captured(Waiting for AFIS)
        //Status 3: Adjudication(AFIS Hit Detected)
        //Status 4: Validated(Cleared - Ready for Print)
        //Status 5: Printed / Exported
        //Status 6: Active / Card Issued
        //Status 7: schedule for approval
        //Status 99: Rejected / Fraud
        //Status 100: DISPLAY ONLY

        // ---------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Retrieves a list of all employees in the system.
        /// </summary>
        /// <returns>A collection of all employee records.</returns>
        [HttpGet("employees")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployees() => Ok(await ExecuteHrListSp(null));

        /// <summary>
        /// Retrieves employees who are pending a biometric capture schedule (Status 0).
        /// </summary>
        [HttpGet("schedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBiometricsSchedule() => Ok(await ExecuteHrListSp(0, unscheduled: true));

        /// <summary>
        /// Retrieves employees who have an active biometric capture schedule (Status 1).
        /// </summary>
        [HttpGet("employees/scheduled")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetScheduled() => Ok(await ExecuteHrListSp(1));

        /// <summary>
        /// Retrieves employees who have been captured for biometric capture (Status 2).
        /// </summary>
        [HttpGet("employees/captured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCaptured() => Ok(await ExecuteHrListSp(2));

        /// <summary>
        /// Retrieves applications flagged with potential AFIS (Automated Fingerprint Identification System) hits (Status 3).
        /// </summary>
        [HttpGet("adjudication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAdjudication() => Ok(await ExecuteHrListSp(3, afisHits: true));

        /// <summary>
        /// Fetches side-by-side data for an applicant and their potential AFIS match for manual adjudication.
        /// </summary>
        /// <param name="currentPersonId">The ID of the new applicant.</param>
        /// <param name="hitPersonId">The ID of the existing record they matched against.</param>
        /// <remarks>
        /// This endpoint returns two objects: 'Applicant' and 'Match'. 
        /// Use this to display photos, signatures, and all 10 fingerprints side-by-side for the HR adjudicator.
        /// </remarks>
        /// <response code="200">Returns comparison data for both records.</response>
        /// <response code="404">If either the applicant or the match record cannot be found.</response>
        [HttpGet("adjudication/compare/{currentPersonId}/{hitPersonId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComparisonData(int currentPersonId, int hitPersonId)
        {
            // 1. Fetch the Current Applicant (Person A)
            var applicant = await _context.EnrollmentRegistries
                .Where(a => a.PersonID == currentPersonId)
                .FirstOrDefaultAsync();

            // 2. Fetch the Matched Record (Person B)
            var match = await _context.EnrollmentRegistries
                .Where(a => a.PersonID == hitPersonId)
                .FirstOrDefaultAsync();

            if (applicant == null || match == null)
            {
                return NotFound(new { message = "One or both records could not be found for comparison." });
            }

            // 3. Return both objects for the frontend to display side-by-side
            return Ok(new
            {
                Applicant = new
                {
                    applicant.PersonID,
                    applicant.ApplicationCode,
                    FullName = $"{applicant.FirstName} {applicant.LastName}",
                    applicant.DepartmentName,
                    applicant.Designation,
                    // Biometrics
                    applicant.Photo,
                    applicant.Signature,
                    Fingers = new
                    {
                        L1 = applicant.LeftThumb,
                        L2 = applicant.LeftIndex,
                        L3 = applicant.LeftMiddle,
                        L4 = applicant.LeftRing,
                        L5 = applicant.LeftSmall,
                        R1 = applicant.RightThumb,
                        R2 = applicant.RightIndex,
                        R3 = applicant.RightMiddle,
                        R4 = applicant.RightRing,
                        R5 = applicant.RightSmall
                    }
                },

                Match = new
                {
                    match.PersonID,
                    match.ApplicationCode,
                    FullName = $"{match.FirstName} {match.LastName}",
                    match.DepartmentName,
                    match.Designation,
                    // Biometrics
                    match.Photo,
                    match.Signature,
                    Fingers = new
                    {
                        L1 = match.LeftThumb,
                        L2 = match.LeftIndex,
                        L3 = match.LeftMiddle,
                        L4 = match.LeftRing,
                        L5 = match.LeftSmall,
                        R1 = match.RightThumb,
                        R2 = match.RightIndex,
                        R3 = match.RightMiddle,
                        R4 = match.RightRing,
                        R5 = match.RightSmall
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves applications that have passed all checks and are ready for ID card printing (Status 4).
        /// </summary>
        [HttpGet("employees/for-printing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForPrinting() => Ok(await ExecuteHrListSp(4));

        /// <summary>
        /// Retrieves a list of ID cards that have been printed but not yet released (Status 5).
        /// </summary>
        [HttpGet("employees/printed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrintedCards() => Ok(await ExecuteHrListSp(5));

        /// <summary>
        /// Retrieves all active employees who have been issued their final ID cards (Status 6).
        /// </summary>
        [HttpGet("employees/active-cards")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveCards() => Ok(await ExecuteHrListSp(6));

        /// <summary>
        /// Retrieves scheduling requests submitted by employees that require HR approval (Status 7).
        /// </summary>
        [HttpGet("schedule/pending-approvals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPendingApprovals() => Ok(await ExecuteHrListSp(7));

        // Helper method to execute stored procedure
        private async Task<IEnumerable<dynamic>> ExecuteHrListSp(int? status, bool unscheduled = false, bool afisHits = false)
        {
            var authUserId = int.TryParse(User.FindFirstValue("id"), out int aid) ? aid : (int?)null;
            var authUsername = User.FindFirstValue("username") ?? "System/API";
            var results = new List<dynamic>();

            var connection = _context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "sp_GetHRDashboardList";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Status", (object)status ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@OnlyUnscheduled", unscheduled));
                command.Parameters.Add(new SqlParameter("@OnlyAFISHits", afisHits));
                command.Parameters.Add(new SqlParameter("@ExecutedByUserId", (object)authUserId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ExecutedByUsername", authUsername));

                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Local helper to safely parse dates
                        string SafeDate(string columnName)
                        {
                            // Check if column exists and has value
                            var val = reader[columnName];
                            if (val == DBNull.Value || val == null) return "";

                            if (DateTime.TryParse(val.ToString(), out DateTime dt))
                                return dt.ToString("MM/dd/yyyy");

                            return "";
                        }

                        results.Add(new
                        {
                            PersonID = reader["PersonID"],
                            ApplicationCode = reader["ApplicationCode"]?.ToString(),
                            EmployeeID = reader["EmployeeID"]?.ToString(),

                            // Use "Surname" because your SP has: LastName AS Surname
                            FullName = $"{reader["FirstName"]} {reader["Surname"]}".Trim(),

                            // Use "DateOfBirth" because your SP has: BirthDate AS DateOfBirth
                            DateOfBirth = SafeDate("DateOfBirth"),

                            Designation = reader["Designation"]?.ToString(),
                            DepartmentName = reader["DepartmentName"]?.ToString(),
                            Status = reader["Status"] != DBNull.Value ? Convert.ToInt32(reader["Status"]) : 0,
                            RequestedDate = SafeDate("DateSchedule")
                        });
                    }
                }
            }
            return results;
        }

        //----------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the initial biometric capture schedule for multiple employees, 
        /// generates unique application codes, creates barcode images, and queues notification emails.
        /// </summary>
        /// <param name="request">Contains PersonIDs and the targeted Schedule Date.</param>
        /// <returns>A status message indicating the number of successfully scheduled records.</returns>
        [HttpPost("schedule/update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleUpdateRequest request)
        {
            int? userId = null;
            string username = User.FindFirstValue("username") ?? "Unknown";

            if (User.Identity.IsAuthenticated && int.TryParse(User.FindFirstValue("id"), out int authenticatedId))
                userId = authenticatedId;

            // Helper for Unified Logging
            void LogAction(string description, string specificNote = null)
            {
                var logEntry = new ApplicationLog
                {
                    LogId = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    LogDescription = description,
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LogTime = DateTime.Now.ToString("HH:mm:ss"),
                    Notes = $"Endpoint: POST /schedule/update. User: {username}. {specificNote}"
                };
                _context.ApplicationLogs.Add(logEntry);
            }

            LogAction("SCHEDULE_UPDATE_ATTEMPT", $"Starting schedule for {request.PersonIDs.Count} IDs. Date: {request.Date:yyyy-MM-dd}.");

            // Barcode Configuration (CODE_128)
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions { Width = 300, Height = 100, Margin = 10, PureBarcode = false }
            };
            barcodeWriter.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");

            string barcodeFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "barcodes");
            if (!Directory.Exists(barcodeFolder)) Directory.CreateDirectory(barcodeFolder);

            int successCount = 0;

            foreach (var personId in request.PersonIDs)
            {
                // 1. Fetch from Unified Registry
                var registryEntry = await _context.EnrollmentRegistries
                    .FirstOrDefaultAsync(p => p.PersonID == personId && p.Status == 0); // Assuming 0 is "Pending"

                if (registryEntry != null)
                {
                    // 2. Update Schedule & Metadata
                    registryEntry.DateSchedule = request.Date;
                    registryEntry.Status = 1; // 1 = Scheduled
                    registryEntry.ApplicationCode = GenerateApplicationCode();

                    // 3. Generate Barcode Image
                    var pixelData = barcodeWriter.Write(registryEntry.ApplicationCode);
                    using var image = new Image<Rgba32>(pixelData.Width, pixelData.Height);

                    // Optimized Pixel Mapping
                    image.ProcessPixelRows(accessor => {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < accessor.Width; x++)
                            {
                                int idx = (y * accessor.Width + x) * 4;
                                row[x] = new Rgba32(pixelData.Pixels[idx], pixelData.Pixels[idx + 1], pixelData.Pixels[idx + 2], pixelData.Pixels[idx + 3]);
                            }
                        }
                    });

                    string fileName = $"{registryEntry.ApplicationCode}.png";
                    string filePath = Path.Combine(barcodeFolder, fileName);
                    image.Save(filePath, new PngEncoder());

                    // 4. Queue Email Notification
                    var domain = _config["BarcodeSettings:PublicBaseUrl"];
                    string publicUrl = $"{domain}/{fileName}";

                    string emailBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <p>Hello {registryEntry.FirstName},</p>
                        <p>Your biometric capture is scheduled for <strong>{registryEntry.DateSchedule:MMMM dd, yyyy}</strong>.</p>
                        <p>Present this barcode at the mobile kit:</p>
                        <img src='{publicUrl}' alt='Barcode' style='width:300px; height:100px;'/>
                        <p style='font-weight:bold;'>{registryEntry.ApplicationCode}</p>
                    </body>
                </html>";

                    _emailQueue.QueueEmail(new EmailMessage
                    {
                        To = registryEntry.Email,
                        Subject = "Biometric Capture Schedule",
                        Body = emailBody
                    });

                    successCount++;
                }
            }

            // 5. Single Save Operation
            LogAction("SCHEDULE_UPDATE_SUCCESS", $"Scheduled {successCount} applications.");
            await _context.SaveChangesAsync();

            return Ok(new { message = "Schedule updated successfully", count = successCount });
        }


        /// <summary>
        /// Updates the scheduled date for existing applications while preserving their original ApplicationCode.
        /// Sends a "Notice of Reschedule" email to the employees.
        /// </summary>
        /// <param name="request">Contains PersonIDs and the new Schedule Date.</param>
        /// <returns>A status message with the success count.</returns>
        [HttpPost("schedule/reschedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reschedule([FromBody] ScheduleUpdateRequest request)
        {
            int? userId = null;
            string username = User.FindFirstValue("username") ?? "Unknown";

            if (User.Identity.IsAuthenticated && int.TryParse(User.FindFirstValue("id"), out int authenticatedId))
                userId = authenticatedId;

            // Unified Logging Helper
            void LogAction(string description, string specificNote = null)
            {
                var logEntry = new ApplicationLog
                {
                    LogId = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    LogDescription = description,
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LogTime = DateTime.Now.ToString("HH:mm:ss"),
                    Notes = $"Endpoint: POST /schedule/reschedule. User: {username}. {specificNote}"
                };
                _context.ApplicationLogs.Add(logEntry);
            }

            LogAction("RESCHEDULE_ATTEMPT", $"Rescheduling {request.PersonIDs.Count} applications to {request.Date:yyyy-MM-dd}.");

            int successCount = 0;

            foreach (var id in request.PersonIDs)
            {
                // Fetch from the Unified Registry using the PersonID
                var registryEntry = await _context.EnrollmentRegistries
                    .FirstOrDefaultAsync(p => p.PersonID == id);

                if (registryEntry != null)
                {
                    // 1. Update the Date only (Keep existing ApplicationCode and Barcode)
                    registryEntry.DateSchedule = request.Date;
                    registryEntry.Status = 1; // Ensure it remains in 'Scheduled' state

                    // 2. Reuse existing Barcode URL
                    var domain = _config["BarcodeSettings:PublicBaseUrl"];
                    string publicUrl = $"{domain}/{registryEntry.ApplicationCode}.png";

                    // 3. Build the Reschedule Email (Using standard Barcode dimensions 300x100)
                    string emailBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; color: #000000; padding: 20px;'>
                        <h2 style='color: #d32f2f;'>Notice of Reschedule</h2>
                        <p>Hello {registryEntry.FirstName},</p>
                        <p>Your biometric capture has been <strong>rescheduled</strong> to:</p>
                        <p style='font-size: 18px; font-weight: bold;'>{registryEntry.DateSchedule:MMMM dd, yyyy}</p>
                        <p>Please present the same barcode previously sent to you, or use the one below:</p>
                        <img src='{publicUrl}' alt='Barcode' style='display:block; margin-top:10px; width:300px; height:100px;'/>
                        <p style='font-size:12px; color:#555555;'>Application Reference: {registryEntry.ApplicationCode}</p>
                    </body>
                </html>";

                    _emailQueue.QueueEmail(new EmailMessage
                    {
                        To = registryEntry.Email,
                        Subject = "RESCHEDULED: Biometric Capture Schedule",
                        Body = emailBody
                    });

                    successCount++;
                }
            }

            // 4. Finalize changes in the single Unified Context
            await _context.SaveChangesAsync();

            LogAction("RESCHEDULE_SUCCESS", $"Successfully rescheduled {successCount} applications.");

            return Ok(new { message = $"Successfully rescheduled {successCount} applications." });
        }

        /// <summary>
        /// Manually validates an application (Status 4) within the unified registry.
        /// This moves the record to the 'Ready for Printing' queue.
        /// </summary>
        /// <param name="request">Contains the specific EmployeeID (or ApplicationCode) to be validated.</param>
        [HttpPost("employees/validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ValidateApplication([FromBody] ValidationRequest request)
        {
            // 1. Validation of Request
            if (request == null || string.IsNullOrEmpty(request.EmployeeId))
            {
                return BadRequest(new { message = "Employee ID is required." });
            }

            // 2. Fetch Record from the Unified Registry
            // We search by EmployeeID, but you could also search by Id or ApplicationCode
            var registryEntry = await _context.EnrollmentRegistries
                .FirstOrDefaultAsync(p => p.EmployeeID == request.EmployeeId);

            if (registryEntry == null)
            {
                return NotFound(new { message = "Application record not found in the registry." });
            }

            // 3. Guard Clauses
            // Check if already validated (4) or printed/completed (5+)
            if (registryEntry.Status >= 4 && registryEntry.Status != 99)
            {
                return BadRequest(new { message = "Application is already validated or has already been processed." });
            }

            // 4. Update Logic
            int oldStatus = registryEntry.Status ?? 0;

            try
            {
                // Update Unified Statuses
                registryEntry.Status = 4;          // Business Status: Validated / Ready for Print
                registryEntry.BiometricStatus = 4; // Technical Status: Cleared
                registryEntry.AFISHit = 0;         // Clear any AFIS flags since HR is manually overriding/validating

                // 5. Log the HR Action
                var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var logEntry = new ApplicationLog
                {
                    LogId = Guid.NewGuid().ToString("N"),
                    UserId = int.TryParse(adminIdClaim, out int id) ? id : null,
                    LogDescription = "APPLICATION_MANUAL_VALIDATION",
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LogTime = DateTime.Now.ToString("HH:mm:ss"),
                    Notes = $"HR Manual Validation for EmployeeID: {request.EmployeeId}. Status moved from {oldStatus} to 4. AppCode: {registryEntry.ApplicationCode}"
                };

                _context.ApplicationLogs.Add(logEntry);

                // 6. Save changes (Atomic update to single table)
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Employee {request.EmployeeId} has been successfully validated and is now ready for printing.",
                    applicationCode = registryEntry.ApplicationCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database update error.", details = ex.Message });
            }
        }

        /// <summary>
        /// Performs a three-way join between Personal Info, Biometrics, and Applications to 
        /// export a PGP-encrypted JSON bundle for card personalization (printing).
        /// Moves application status to 'Ready for Printing' (Status 5).
        /// </summary>
        /// <param name="selectedCodes">List of PersonIDs to be included in the export.</param>
        /// <returns>An encrypted .pgp file containing sensitive employee data.</returns>
        [HttpPost("print-cards/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportSelectedCardsToEncryptedJson([FromBody] List<int> selectedCodes)
        {
            if (selectedCodes == null || !selectedCodes.Any())
                return BadRequest("No codes selected.");

            var adminIdClaim = User.FindFirst("id")?.Value;
            int? userId = int.TryParse(adminIdClaim, out int id) ? id : null;
            string username = User.FindFirstValue("username") ?? "System";

            try
            {
                // 1. Join EnrollmentRegistry with PersonalInformation
                // This ensures the ID card has the most up-to-date HR data (Name, Address, etc.)
                var payload = await _context.EnrollmentRegistries
                    .Where(r => selectedCodes.Contains(r.PersonID))
                    .Join(_context.PersonalInformations,
                        reg => reg.PersonID,
                        pers => pers.PersonID,
                        (reg, pers) => new PersoExportDto
                        {
                            // Data from Master PersonalInformation
                            AgencyEmployeeNo = pers.AgencyEmployeeNo,
                            Surname = pers.Surname,
                            FirstName = pers.FirstName,
                            MiddleName = pers.MiddleName,
                            NameExtension = pers.NameExtension,
                            DateOfBirth = pers.DateOfBirth.HasValue ? pers.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                            SexName = pers.SexID == 1 ? "Male" : "Female",
                            Citizenship = pers.Citizenship ?? "Filipino",
                            BloodType = pers.BloodType ?? "N/A",
                            //ResidentialAddress = pers.ResidentialAddress,
                            //ResidentialZip = pers.ResidentialZip,
                            Email = pers.Email,
                            MobileNo = pers.MobileNo,

                            // Data from Registry (Job/Biometrics)
                            DepartmentName = reg.DepartmentName,
                            Designation = reg.Designation,
                            Photo = reg.Photo,
                            Signature = reg.Signature,
                            DateCapture = reg.DateCapture.HasValue ? reg.DateCapture.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,

                            Iso_Fingerprints = new FingerprintSetDto
                            {
                                LeftThumb = reg.ISO_L_Thumb,
                                LeftIndex = reg.ISO_L_Index,
                                LeftMiddle = reg.ISO_L_Middle,
                                LeftRing = reg.ISO_L_Ring,
                                LeftSmall = reg.ISO_L_Small,
                                RightThumb = reg.ISO_R_Thumb,
                                RightIndex = reg.ISO_R_Index,
                                RightMiddle = reg.ISO_R_Middle,
                                RightRing = reg.ISO_R_Ring,
                                RightSmall = reg.ISO_R_Small
                            },

                            Afis_Fingerprints = new FingerprintSetDto
                            {
                                LeftThumb = reg.AFIS_L_Thumb,
                                LeftIndex = reg.AFIS_L_Index,
                                LeftMiddle = reg.AFIS_L_Middle,
                                LeftRing = reg.AFIS_L_Ring,
                                LeftSmall = reg.AFIS_L_Small,
                                RightThumb = reg.AFIS_R_Thumb,
                                RightIndex = reg.AFIS_R_Index,
                                RightMiddle = reg.AFIS_R_Middle,
                                RightRing = reg.AFIS_R_Ring,
                                RightSmall = reg.AFIS_R_Small
                            },

                            EyeLeft = reg.EyeLeft,
                            EyeRight = reg.EyeRight
                        })
                    .ToListAsync();

                if (!payload.Any()) return NotFound("No matching records found.");

                // 2. Update Status and Log the Export
                var registryRecords = await _context.EnrollmentRegistries
                    .Where(r => selectedCodes.Contains(r.PersonID))
                    .ToListAsync();

                foreach (var record in registryRecords)
                {
                    record.Status = 5; // Exported for Printing
                    record.BiometricStatus = 5;

                    // Log each individual export for auditing
                    LogAdjudicationDecision(record.ApplicationCode, "EXPORT_TO_PRINTER", "Batch export for card personalization.", userId, username);
                }

                // 3. Encrypt and Save
                byte[] encryptedData = await _pgpService.EncryptDataPrepBundleAsync(payload);
                await _context.SaveChangesAsync();

                string fileName = $"DataPrepBundle_{DateTime.Now:yyyyMMdd_HHmm}.pgp";
                return File(encryptedData, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        //afis decision log
        private void LogAdjudicationDecision(string applicationCode, string decision, string remarks, int? userId, string username)
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = userId, // Authenticated HR User ID
                LogDescription = "ADJUDICATION_DECISION",
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                // Structured notes for easy auditing
                Notes = $"[Adjudication] Code: {applicationCode} | Result: {decision} | Remarks: {remarks} | Performed by: {username}"
            };

            _context.ApplicationLogs.Add(logEntry);
        }

        /// <summary>
        /// Finalizes the decision on AFIS (Automated Fingerprint Identification System) hits.
        /// Confirms a hit (Status 99/Rejected) or clears it as a false positive (Status 4/Cleared).
        /// </summary>
        /// <param name="decision">Contains the ApplicationCode and the HR adjudication decision.</param>
        /// <returns>Success confirmation message.</returns>
        /// <summary>
        /// Finalizes the decision on AFIS hits within the unified registry.
        /// Confirms a hit (Status 99/Rejected) or clears it as a false positive (Status 4/Cleared).
        /// </summary>
        [HttpPost("adjudicate/decision")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetAdjudicationDecision([FromBody] AdjudicationDecisionDto decision)
        {
            // 1. Get User Context from Claims
            var currentUsername = User.FindFirst("username")?.Value ?? "Unknown";
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int id) ? id : null;

            // 2. Fetch Single Unified Record
            var registryEntry = await _context.EnrollmentRegistries
                .FirstOrDefaultAsync(e => e.ApplicationCode == decision.ApplicationCode);

            if (registryEntry == null)
                return NotFound(new { message = "Application record not found in Unified Registry." });

            // 3. Determine Decision Logic
            // If IsConfirmedHit is true: Status 99 (Rejected/Duplicate)
            // If IsConfirmedHit is false: Status 4 (Cleared/Validated)
            string decisionText = decision.IsConfirmedHit ? "CONFIRMED_HIT (REJECTED)" : "FALSE_POSITIVE (CLEARED)";
            int finalStatus = decision.IsConfirmedHit ? 99 : 4;

            try
            {
                // 4. Update Statuses in one table
                registryEntry.Status = finalStatus;
                registryEntry.BiometricStatus = finalStatus;

                // Reset the AFIS flag if the HR expert determines it's a false positive
                if (!decision.IsConfirmedHit)
                {
                    registryEntry.AFISHit = 0;
                }

                // 5. Audit Logging
                // This uses the helper method you provided to maintain the audit trail
                LogAdjudicationDecision(
                    decision.ApplicationCode,
                    decisionText,
                    decision.Remarks,
                    userId,
                    currentUsername
                );

                // 6. Atomic Save
                // Standard SaveChangesAsync is transactional for a single context
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Decision '{decisionText}' saved successfully.",
                    applicationCode = decision.ApplicationCode,
                    newStatus = finalStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving adjudication decision", error = ex.Message });
            }
        }

        /// <summary>
        /// Approves schedule requests in the Unified Registry (Status 7 -> 1).
        /// Generates the official barcode and sends the approval email.
        /// </summary>
        [HttpPost("schedule/approve-request")]
        public async Task<IActionResult> ApproveSchedule([FromBody] List<int> personIds)
        {
            var adminIdClaim = User.FindFirstValue("id");
            int adminId = int.TryParse(adminIdClaim, out int id) ? id : 0;
            var username = User.FindFirstValue("username") ?? "Admin";

            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions { Width = 300, Height = 100, Margin = 10 }
            };

            string barcodeFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "barcodes");
            if (!Directory.Exists(barcodeFolder)) Directory.CreateDirectory(barcodeFolder);

            int successCount = 0;

            foreach (var personId in personIds)
            {
                var registryEntry = await _context.EnrollmentRegistries
                    .FirstOrDefaultAsync(r => r.PersonID == personId && r.Status == 7);

                if (registryEntry != null)
                {
                    // --- FIX 1: Ensure ApplicationCode is NOT empty ---
                    if (string.IsNullOrWhiteSpace(registryEntry.ApplicationCode))
                    {
                        // Fallback: Generate a code if it somehow got lost
                        registryEntry.ApplicationCode = GenerateApplicationCode();
                    }

                    registryEntry.Status = 1;
                    // Explicitly set as modified to ensure EF tracks it
                    _context.Entry(registryEntry).State = EntityState.Modified;

                    try
                    {
                        // This is where the ZXing crash happened
                        var pixelData = barcodeWriter.Write(registryEntry.ApplicationCode);

                        using (var image = new Image<Rgba32>(pixelData.Width, pixelData.Height))
                        {
                            for (int y = 0; y < pixelData.Height; y++)
                            {
                                for (int x = 0; x < pixelData.Width; x++)
                                {
                                    int idx = (y * pixelData.Width + x) * 4;
                                    image[x, y] = new Rgba32(
                                        pixelData.Pixels[idx],
                                        pixelData.Pixels[idx + 1],
                                        pixelData.Pixels[idx + 2],
                                        pixelData.Pixels[idx + 3]);
                                }
                            }
                            string fileName = $"{registryEntry.ApplicationCode}.png";
                            image.Save(Path.Combine(barcodeFolder, fileName), new PngEncoder());
                        }

                        // --- FIX 2: Check Email before Queuing ---
                        if (!string.IsNullOrWhiteSpace(registryEntry.Email))
                        {
                            var publicUrl = $"{_config["BarcodeSettings:PublicBaseUrl"]}/{registryEntry.ApplicationCode}.png";
                            _emailQueue.QueueEmail(new EmailMessage
                            {
                                To = registryEntry.Email,
                                Subject = "APPROVED: Biometric Capture Schedule",
                                Body = $@"<html>... {registryEntry.ApplicationCode} ...</html>" // Shortened for brevity
                            });
                        }

                        _context.ApplicationLogs.Add(new ApplicationLog
                        {
                            LogId = Guid.NewGuid().ToString("N"),
                            UserId = adminId,
                            LogDescription = "SCHEDULE_APPROVED_BY_HR",
                            LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                            LogTime = DateTime.Now.ToString("HH:mm:ss"),
                            Notes = $"Approved PersonID: {personId} | Code: {registryEntry.ApplicationCode}"
                        });

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log individual failure but allow the loop to continue for others
                        Console.WriteLine($"Error processing PersonID {personId}: {ex.Message}");
                        _context.Entry(registryEntry).State = EntityState.Unchanged;
                        continue;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"{successCount} schedule requests approved." });
        }


        // ---------------------------------------------------------------------------------------------------
        // -------------------------------- EMPLOYEE WORK SCHEDULE  ------------------------------------------
        // ---------------------------------------------------------------------------------------------------
        [HttpPost("shift-templates")]
        public async Task<IActionResult> CreateShiftTemplate([FromBody] BulkScheduleTemplateDto dto)
        {
            if (dto == null || dto.Days == null) return BadRequest("Template data is missing.");

            var template = new ScheduleTemplate
            {
                TemplateName = dto.TemplateName,
                // Use SelectMany to flatten the list of DayOfWeek integers into individual DaySchedule objects
                Days = dto.Days.SelectMany(dDto => dDto.DaysOfWeek.Select(dayVal => new DaySchedule
                {
                    DayOfWeek = dayVal,
                    IsRestDay = dDto.IsRestDay,
                    ShiftSegments = dDto.Shifts.Select(s => new ShiftSegment
                    {
                        ShiftType = s.ShiftType,
                        StartTime = TimeSpan.Parse(s.StartTime),
                        EndTime = TimeSpan.Parse(s.EndTime)
                    }).ToList()
                })).ToList()
            };

            _db.ScheduleTemplates.Add(template);

            try
            {
                await _db.SaveChangesAsync();
                return Ok(new { TemplateId = template.TemplateId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving template: {ex.Message}");
            }
        }

        [HttpPost("assign-shift")]
        public async Task<IActionResult> AssignShift([FromBody] AssignShiftDto dto)
        {
            // Use an explicit transaction
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Deactivate current assignments
                var current = await _db.EmployeeScheduleAssignments
                    .Where(a => a.EmployeeID == dto.EmployeeID && a.IsActive)
                    .ToListAsync();

                foreach (var a in current)
                {
                    a.IsActive = false;
                }

                // 2. Create new assignment
                _db.EmployeeScheduleAssignments.Add(new EmployeeScheduleAssignment
                {
                    EmployeeID = dto.EmployeeID,
                    TemplateId = dto.TemplateId,
                    EffectiveDate = dto.EffectiveDate,
                    IsActive = true,
                    //CreatedAt = DateTime.Now
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Assignment successful." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Assignment failed.", error = ex.Message });
            }
        }

        [HttpGet("shift-templates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetShiftTemplates()
        {
            var templates = await _db.ScheduleTemplates
                .AsNoTracking()
                .Include(t => t.Days)
                    .ThenInclude(d => d.ShiftSegments)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TemplateId,
                    t.TemplateName,
                    t.CreatedAt,
                    Days = t.Days.Select(d => new
                    {
                        d.DayOfWeek,
                        d.IsRestDay,
                        Shifts = d.ShiftSegments.Select(s => new
                        {
                            s.ShiftType,
                            StartTime = s.StartTime.ToString(@"hh\:mm"), // Format for input fields
                            EndTime = s.EndTime.ToString(@"hh\:mm")
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(templates);
        }

        /// <summary>
        /// Retrieves all employee schedules, sorted by the official Day of Week order.
        /// </summary>
        [HttpGet("all-work-schedules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllEmployeeSchedules() => await FetchSchedules(null);

        /// <summary>
        /// Generates a daily attendance report for all employees on a specific date.
        /// </summary>
        /// <param name="date">The target date (defaults to Today if not provided).</param>
        /// <remarks>
        /// This report joins processed logs with employee information to show "Absent" vs "Present" statuses.
        /// </remarks>
        [HttpGet("work-schedules/by-department/{deptName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSchedulesByDepartment(string deptName) => await FetchSchedules(deptName);

        private async Task<IActionResult> FetchSchedules(string deptName)
        {
            var empQuery = _db.EnrollmentRegistries.AsNoTracking();
            if (!string.IsNullOrEmpty(deptName))
                empQuery = empQuery.Where(a => a.DepartmentName == deptName);

            var employees = await empQuery.ToListAsync();
            if (!employees.Any()) return NotFound(new { message = "No employees found." });

            var empIds = employees.Select(e => e.EmployeeID).ToList();
            var activeAssignments = await _db.EmployeeScheduleAssignments
                .AsNoTracking()
                .Include(a => a.Template)
                    .ThenInclude(t => t.Days)
                        .ThenInclude(d => d.ShiftSegments)
                .Where(a => a.IsActive && empIds.Contains(a.EmployeeID))
                .ToListAsync();

            var response = employees.Select(emp =>
            {
                // Safely get IDs for comparison
                var empId = emp.EmployeeID?.Trim() ?? "UNKNOWN";
                var firstName = emp.FirstName?.Trim() ?? "";
                var lastName = emp.LastName?.Trim() ?? "";

                var assignment = activeAssignments.FirstOrDefault(a =>
                    !string.IsNullOrEmpty(a.EmployeeID) &&
                    a.EmployeeID.Trim() == empId);

                return new DisplayScheduleDto
                {
                    EmployeeID = empId,
                    FullName = $"{firstName} {lastName}".Trim(),
                    Department = emp.DepartmentName ?? "N/A",
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
                                    ShiftName = s.ShiftType ?? "Unknown",
                                    TimeRange = $"{DateTime.Today.Add(s.StartTime):hh:mm tt} - {DateTime.Today.Add(s.EndTime):hh:mm tt}"
                                }).ToList()
                        };
                    }).ToList()
                };
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the work schedule for a specific employee.
        /// </summary>
        [HttpGet("work-schedules/employee/{employeeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetScheduleByEmployeeId(string employeeId)
        {
            // 1. Fetch the single employee
            var employee = await _db.EnrollmentRegistries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID.Trim() == employeeId.Trim());

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {employeeId} not found." });

            // 2. Fetch only their active assignment
            var assignment = await _db.EmployeeScheduleAssignments
                .AsNoTracking()
                .Include(a => a.Template)
                    .ThenInclude(t => t.Days)
                        .ThenInclude(d => d.ShiftSegments)
                .Where(a => a.IsActive && a.EmployeeID.Trim() == employeeId.Trim())
                .FirstOrDefaultAsync();

            // 3. Map to DTO (Reuse logic)
            var result = new DisplayScheduleDto
            {
                EmployeeID = employee.EmployeeID.Trim(),
                FullName = $"{employee.FirstName?.Trim()} {employee.LastName?.Trim()}".Trim(),
                Department = employee.DepartmentName ?? "N/A",
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
                                ShiftName = s.ShiftType ?? "Unknown",
                                TimeRange = $"{DateTime.Today.Add(s.StartTime):hh:mm tt} - {DateTime.Today.Add(s.EndTime):hh:mm tt}"
                            }).ToList()
                    };
                }).ToList()
            };

            return Ok(result);
        }

        [HttpGet("hr/daily-attendance")]
        public IActionResult GetAttendanceLogs(
          [FromQuery] DateTime? date,
          [FromQuery] string? employeeId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 50)
        {
            // 1. Build the base query - Removed mandatory .Where(l => l.WorkDate.Date == targetDate)
            var query = _db.AttendanceLogs_Processed.AsQueryable();

            // Conditionally filter by date ONLY if provided
            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(l => l.WorkDate.Date == targetDate);
            }

            // Conditionally filter by employeeId if provided
            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                query = query.Where(l => l.EmployeeID == employeeId);
            }

            // 2. Get the total count for pagination
            var totalCount = query.Count();

            // 3. Apply Pagination (Always order by WorkDate DESC so the newest logs appear first)
            var logs = query
                .OrderByDescending(l => l.WorkDate)
                .ThenBy(l => l.EmployeeID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable()
                .Select(l => new
                {
                    l.EmployeeID,
                    WorkDate = l.WorkDate.ToString("yyyy-MM-dd"),
                    TimeIn = l.MorningIn?.ToString("HH:mm:ss"),
                    Break = l.BreakIn?.ToString("HH:mm:ss"),
                    TimeOut = l.AfternoonOut?.ToString("HH:mm:ss")
                })
                .ToList();

            // 4. Return metadata
            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }

        [HttpPost("upload-and-process")]
        public async Task<IActionResult> UploadAndProcess(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty.");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // STEP 1: Bulk copy file to RawAttendanceLogs
                try
                {
                    // Open the file stream
                    using (var stream = file.OpenReadStream())
                    using (var reader = new StreamReader(stream))
                    {
                        // We pass the active 'reader' into the helper
                        var logs = GetLogsFromStream(reader);

                        // Initialize ObjectReader using the 'logs' enumerable
                        using (var objectReader = ObjectReader.Create(logs,
                            "EmployeeID", "Timestamp", "DeviceID", "LogType", "StatusFlag", "AdditionalFlag"))
                        {
                            using (var bulkCopy = new SqlBulkCopy(connection))
                            {
                                bulkCopy.DestinationTableName = "dbo.RawAttendanceLogs";
                                bulkCopy.BatchSize = 5000;
                                bulkCopy.BulkCopyTimeout = 300; // Match SP timeout

                                // Map properties to SQL columns
                                bulkCopy.ColumnMappings.Add("EmployeeID", "EmployeeID");
                                bulkCopy.ColumnMappings.Add("Timestamp", "Timestamp");
                                bulkCopy.ColumnMappings.Add("DeviceID", "DeviceID");
                                bulkCopy.ColumnMappings.Add("LogType", "LogType");
                                bulkCopy.ColumnMappings.Add("StatusFlag", "StatusFlag");
                                bulkCopy.ColumnMappings.Add("AdditionalFlag", "AdditionalFlag");

                                await bulkCopy.WriteToServerAsync(objectReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Staging failed: {ex.Message}");
                }

                // STEP 2: Now that Raw is populated, call the Transformation SP
                try
                {
                    using (var command = new SqlCommand("sp_TransformRawToProcessed", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 300;
                        await command.ExecuteNonQueryAsync();
                    }
                    return Ok("File uploaded and processed successfully.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Transformation failed: {ex.Message}");
                }
            }
        }

        // Helper to yield records (Streaming)
        private IEnumerable<RawAttendanceLogDto> GetLogsFromStream(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Trim().Split('\t', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 6)
                {
                    // Use TryParse to prevent the whole import from failing on one bad line
                    if (DateTime.TryParse(parts[1], out DateTime dt) && int.TryParse(parts[3], out int logType))
                    {
                        yield return new RawAttendanceLogDto
                        {
                            EmployeeID = parts[0],
                            Timestamp = dt,
                            DeviceID = parts[2],
                            LogType = logType,
                            StatusFlag = parts[4],
                            AdditionalFlag = parts[5]
                        };
                    }
                }
            }
        }

    [HttpGet("hr/dtr-report")]
    public async Task<IActionResult> GetDtrReport(
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] string? employeeId)
        {
            var start = (startDate ?? DateTime.Today).Date;
            var end = (endDate ?? startDate ?? DateTime.Today).Date;

            // 1. Fetch the raw data from the DB using a query that EF can translate
            var query = from dtr in _db.DailyTimeRecords
                        join emp in _db.EnrollmentRegistries on dtr.EmployeeID.Trim() equals emp.EmployeeID.Trim()
                        where dtr.WorkDate.Date >= start && dtr.WorkDate.Date <= end
                        select new
                        {
                            dtr.DtrId,
                            dtr.EmployeeID,
                            emp.FirstName, // Fetch columns separately
                            emp.LastName,
                            emp.DepartmentName,
                            dtr.WorkDate,
                            dtr.TotalRegularHours,
                            dtr.TotalOvertimeHours,
                            dtr.AttendanceStatus
                        };

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                query = query.Where(q => q.EmployeeID.Trim() == employeeId.Trim());
            }

            // 2. Execute to List first (this switches to client-side)
            var rawData = await query.ToListAsync();

            // 3. Perform the string concatenation in-memory
            var report = rawData
                .Select(d => new
                {
                    d.DtrId,
                    d.EmployeeID,
                    FullName = $"{d.FirstName.Trim()} {d.LastName.Trim()}",
                    d.DepartmentName,
                    d.WorkDate,
                    d.TotalRegularHours,
                    d.TotalOvertimeHours,
                    d.AttendanceStatus
                })
                .OrderByDescending(q => q.WorkDate)
                .ThenBy(q => q.FullName)
                .ToList();

            return Ok(report);
        }

        //----------------------------------------------------------------------------------

        /// <summary>
        /// Provides a snapshot of the current counts for all application statuses.
        /// </summary>
        /// <remarks>
        /// Use this for the "Stats Counters" or "Cards" on the HR Dashboard home page.
        /// It includes counts for Pending (0), Scheduled (1), Adjudication (3), etc.
        /// </remarks>
        [HttpGet("id-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIDStats()
        {
            var stats = await _db.EnrollmentRegistries
                .GroupBy(e => 1)
                .Select(g => new
                {
                    TotalEmployees = g.Count(),
                    ForSchedule = g.Count(e => e.Status == 0),
                    Scheduled = g.Count(e => e.Status == 1),
                    CapturedPendingAFIS = g.Count(e => e.Status == 2),
                    PendingAdjudication = g.Count(e => e.Status == 3), // AFIS Hits
                    ReadyForPrinting = g.Count(e => e.Status == 4),    // Cleared
                    Printed = g.Count(e => e.Status == 5),
                    ActiveCards = g.Count(e => e.Status == 6),

                    // --- Added Status 7 ---
                    ScheduleRequests = g.Count(e => e.Status == 7)     // Awaiting HR Approval
                })
                .FirstOrDefaultAsync()
                ?? new
                {
                    TotalEmployees = 0,
                    ForSchedule = 0,
                    Scheduled = 0,
                    CapturedPendingAFIS = 0,
                    PendingAdjudication = 0,
                    ReadyForPrinting = 0,
                    Printed = 0,
                    ActiveCards = 0,
                    ScheduleRequests = 0
                };

            return Ok(stats);
        }

        //----------------------------------------------------------------------------------
        private string GenerateApplicationCode()
        {
            // Example: MKT-20251011-ABC123
            return $"MKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

    }
}
