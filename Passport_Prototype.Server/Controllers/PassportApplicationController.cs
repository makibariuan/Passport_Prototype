using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using SeniorCitizen.Server.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Claims;
using ZXing;
using ZXing.Common;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmployeesDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;

        private const string TempFolder = "temp_uploads";

        public ApplicationController(
            AppDbContext context,
            IWebHostEnvironment env,
            IFileService fileService,
            EmployeesDbContext db)
        {
            _context = context;
            _env = env;
            _fileService = fileService;
            _db = db;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateApplication([FromForm] CreateApplicationDTO dto)
        {
            try
            {
                var userIdString = User.FindFirstValue("id");

                if (!int.TryParse(userIdString, out int UserId))
                    return BadRequest("Invalid user ID in claims.");

                // ✅ FIX: Safe WebRootPath
                string webRoot = _env.WebRootPath
                    ?? Path.Combine(_env.ContentRootPath, "wwwroot");

                string tempPath = Path.Combine(webRoot, "temp_uploads");
                string barcodeFolder = Path.Combine(webRoot, "barcodes");

                Directory.CreateDirectory(tempPath);
                Directory.CreateDirectory(barcodeFolder);

                // ✅ Validate files
                if (dto.ValidId == null || dto.Certificate == null)
                    return BadRequest("ValidId and Certificate are required.");

                // ✅ Save temp files
                async Task<string> SaveTempFile(IFormFile file)
                {
                    if (file.Length == 0)
                        throw new Exception("Empty file uploaded.");

                    string ext = Path.GetExtension(file.FileName);
                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string fullPath = Path.Combine(tempPath, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    return fileName;
                }

                string validIdKey = await SaveTempFile(dto.ValidId);
                string certificateKey = await SaveTempFile(dto.Certificate);

                // ✅ Finalize upload (IMPORTANT: dapat fixed din sa FileService)
                string validIdPath = await _fileService.FinalizeUpload(validIdKey, UserId);
                string certificatePath = await _fileService.FinalizeUpload(certificateKey, UserId);

                // ✅ Generate Application Code
                string sharedCode = GenerateApplicationCode();

                // ✅ Generate Barcode (SAFE)
                string barcodeDbPath;
                try
                {
                    var barcodeWriter = new BarcodeWriterPixelData
                    {
                        Format = BarcodeFormat.CODE_128,
                        Options = new EncodingOptions
                        {
                            Width = 300,
                            Height = 100,
                            Margin = 10,
                            PureBarcode = false
                        }
                    };

                    barcodeWriter.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");

                    var pixelData = barcodeWriter.Write(sharedCode);

                    using var image = new Image<Rgba32>(pixelData.Width, pixelData.Height);

                    image.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < accessor.Width; x++)
                            {
                                int idx = (y * accessor.Width + x) * 4;
                                row[x] = new Rgba32(
                                    pixelData.Pixels[idx],
                                    pixelData.Pixels[idx + 1],
                                    pixelData.Pixels[idx + 2],
                                    pixelData.Pixels[idx + 3]
                                );
                            }
                        }
                    });

                    string barcodeFileName = $"{sharedCode}.png";
                    string barcodeFullPath = Path.Combine(barcodeFolder, barcodeFileName);

                    image.Save(barcodeFullPath, new PngEncoder());

                    barcodeDbPath = $"/barcodes/{barcodeFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Barcode generation failed: " + ex.Message);
                }

                // ✅ Create Application
                var application = new Application
                {
                    UserId = UserId,
                    PassportPersonalInformationId = (int)dto.PassportPersonalInformationId!,
                    Region = dto.Region,
                    Country = dto.Country,
                    Site = dto.Site,
                    Schedule = dto.Schedule,
                    ApplicationType = dto.ApplicationType,
                    CitizenshipBasis = dto.CitizenshipBasis,
                    isForeignPassportHolder = dto.isForeignPassportHolder,
                    isCourtesyLane = dto.isCourtesyLane,
                    DocumentType = dto.DocumentType,
                    IdDocumentIdNumber = dto.IdDocumentIdNumber,
                    IdDocumentType = dto.IdDocumentType,
                    ValidIdPath = validIdPath,
                    CertificatePath = certificatePath,
                    ProcessingType = dto.ProcessingType,
                    PaymentMethod = dto.PaymentMethod,
                    DeliveryOption = dto.DeliveryOption,
                    Amount = dto.Amount,
                    isPaid = false,
                    ApplicationStatus = 1,
                    ApplicationCode = sharedCode,
                    ApplicationBarCodePath = barcodeDbPath
                };

                var registryEntry = new EnrollmentRegistryID
                {
                    PersonID = UserId,
                    ApplicationCode = sharedCode,
                    ApplicationType = 2,
                    Status = 1,
                    CreatedAt = DateTime.Now,
                    DateSchedule = dto.Schedule
                };

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await _context.Applications.AddAsync(application);
                    await _context.SaveChangesAsync();

                    await _db.EnrollmentRegistries.AddAsync(registryEntry);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { application, registryCode = sharedCode });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Database error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unexpected error: " + ex.ToString());
            }
        }

        private string GenerateApplicationCode()
        {
            // Example: MKT-20251011-ABC123
            return $"MKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }




        [HttpGet("GetApplicationsWithUserInfo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationsWithUserInfo()
        {
            var result = await (
                from app in _context.Applications

                join enrollment in _context.EnrollmentRegistries
                  on app.ApplicationId equals enrollment.Id

                select new
                {
                    // Enrollment Registry fields
                    EnrollmentId = enrollment.Id,
                    EnrollmentAccessCode = enrollment.ApplicationCode,

                    PersonID = enrollment.PersonID,
                    FirstName = enrollment.FirstName,
                    MiddleName = enrollment.MiddleName,
                    LastName = enrollment.LastName,
                    BirthDate = enrollment.BirthDate,
                    Email = enrollment.Email,
                    EmployeeID = enrollment.EmployeeID,
                    DepartmentName = enrollment.DepartmentName,
                    Designation = enrollment.Designation,
                    CitizenType = enrollment.CitizenType,
                    EnrollmentStatus = enrollment.Status,
                    EnrollmentCreatedAt = enrollment.CreatedAt,
                    Photo = enrollment.Photo,
                    Signature = enrollment.Signature,
                    // Add more enrollment fields if needed

                    // Application fields
                    ApplicationId = app.ApplicationId,
                    UserId = app.UserId,
                    Region = app.Region,
                    Country = app.Country,
                    Site = app.Site,
                    Schedule = app.Schedule,
                    AppType = app.ApplicationType,
                    CitizenshipBasis = app.CitizenshipBasis,
                    isForeignPassportHolder = app.isForeignPassportHolder,
                    isCourtesyLane = app.isCourtesyLane,
                    DocumentType = app.DocumentType,
                    //
                    IdDocumentIdNumber = app.IdDocumentIdNumber,
                    ValidIdPath = app.ValidIdPath,
                    CertificatePath = app.CertificatePath,
                    ProcessingType = app.ProcessingType,
                    ApplicationBarCodePath = app.ApplicationBarCodePath,

                    PaymentMethod = app.PaymentMethod,
                    DeliveryOption = app.DeliveryOption,

                    isPaid = app.isPaid,
                    ApplicationStatus = app.ApplicationStatus,
                    // Add more application fields if needed
                }
            ).ToListAsync();

            return Ok(result);
        }

        [HttpGet("My-Applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID in claims.");

            var applications = await (
                from app in _context.Applications
                join ppi in _context.PassportPersonalInformation
                    on app.PassportPersonalInformationId equals ppi.PassportPersonalInformationId into ppiJoin
                from ppi in ppiJoin.DefaultIfEmpty() // LEFT JOIN to allow missing profiles
                where app.UserId == userId
                select new
                {
                    applicationId = app.ApplicationId,
                    site = app.Site ?? "",  
                    status = (int?)app.ApplicationStatus,
                    barcodePath = app.ApplicationBarCodePath ?? "",
                    barcode = app.ApplicationBarCodePath ?? "",
                    profileName = ppi != null
                        ? $"{ppi.FirstName} {(ppi.MiddleName ?? "")} {ppi.LastName} {(ppi.Suffix ?? "")}".Trim()
                        : "No Profile",
                    schedule = app.Schedule
                }
            ).ToListAsync();

            return Ok(applications);
        }

        [HttpGet("{applicationId}")]
        [Authorize]
        public async Task<IActionResult> GetApplicationById(int applicationId)
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID in claims.");

            var application = await (
                from app in _context.Applications
                join ppi in _context.PassportPersonalInformation
                    on app.PassportPersonalInformationId equals ppi.PassportPersonalInformationId into ppiJoin
                from ppi in ppiJoin.DefaultIfEmpty() // LEFT JOIN
                where app.ApplicationId == applicationId && app.UserId == userId
                select new
                {
                    applicationId = app.ApplicationId,
                    status = (int?)app.ApplicationStatus,
                    barcodePath = app.ApplicationBarCodePath ?? "",
                    barcode = app.ApplicationBarCodePath ?? "",
                    profileName = ppi != null
                        ? $"{ppi.FirstName} {(ppi.MiddleName ?? "")} {ppi.LastName} {(ppi.Suffix ?? "")}".Trim()
                        : "No Profile",
                    site = app.Site ?? "",
                    schedule = app.Schedule
                }
            ).FirstOrDefaultAsync();

            if (application == null)
                return NotFound();

            return Ok(application);
        }
    }

}
