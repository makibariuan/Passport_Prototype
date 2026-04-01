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
using OnlineRegistration.Server.Services.Interfaces; // Make sure IFileService is in this namespace


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
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int UserId))
            {
                throw new Exception("Invalid user ID in claims.");
            }

            // 1. Handle File Uploads
            string tempPath = Path.Combine(_env.WebRootPath, "temp_uploads");
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

            async Task<string> SaveTempFile(IFormFile file)
            {
                string ext = Path.GetExtension(file.FileName);
                string fileName = $"{Guid.NewGuid()}{ext}";
                string fullPath = Path.Combine(tempPath, fileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
                return fileName;
            }

            string validIdKey = await SaveTempFile(dto.ValidId);
            string certificateKey = await SaveTempFile(dto.Certificate);

            string validIdPath = await _fileService.FinalizeUpload(validIdKey, UserId!);
            string certificatePath = await _fileService.FinalizeUpload(certificateKey, UserId);

            // 2. Generate a Shared Application Code
            string sharedCode = GenerateApplicationCode();

            // 2.5 Generate Barcode
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

            // Ensure folder exists
            string barcodeFolder = Path.Combine(_env.WebRootPath, "barcodes");
            if (!Directory.Exists(barcodeFolder))
                Directory.CreateDirectory(barcodeFolder);

            // Generate barcode image
            var pixelData = barcodeWriter.Write(sharedCode);

            using var image = new Image<Rgba32>(pixelData.Width, pixelData.Height);

            // Map pixels
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

            // Save file
            string barcodeFileName = $"{sharedCode}.png";
            string barcodeFullPath = Path.Combine(barcodeFolder, barcodeFileName);
            image.Save(barcodeFullPath, new PngEncoder());

            // This is what you store in DB (relative path)
            string barcodeDbPath = $"/barcodes/{barcodeFileName}";

            // 3. Create Passport Application Entity
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

            // 4. Create Registry Entry
            var registryEntry = new EnrollmentRegistryID
            {
                PersonID = UserId!,
                ApplicationCode = sharedCode,
                ApplicationType = 2, 

                Status = 1, // Pending/Active
                CreatedAt = DateTime.Now,
                DateSchedule = dto.Schedule
            };

            // 5. Transactional Save
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
                return StatusCode(500, "Error saving data to multiple registries: " + ex.Message);
            }
        }

        private string GenerateApplicationCode()
        {
            // Example: MKT-20251011-ABC123
            return $"DFA-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }




        [HttpGet("GetApplicationsWithUserInfo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationsWithUserInfo()
        {
            var result = await _context.ApplicationWithUserInfoDtos
                .FromSqlRaw(@"
            WITH Ranked AS (
                SELECT 
                    e.[ApplicationCode],
                    p.[PassportPersonalInformationId],
                    p.[FirstName] AS PassportFirstName,
                    p.[MiddleName] AS PassportMiddleName,
                    p.[LastName] AS PassportLastName,
                    p.[BirthCity],
                    p.[BirthBarangay],
                    p.[BirthLegitimacy],
                    p.[Relationship],
                    p.[IsAdoptee],

                    e.[Id],
                    e.[PersonID],
                    e.[ApplicationType],
                    e.[FirstName] AS EnrollmentFirstName,
                    e.[MiddleName] AS EnrollmentMiddleName,
                    e.[LastName] AS EnrollmentLastName,
                    e.[AFISPersonHit],
                    e.[BiometricStatus],

                    a.[ApplicationId],
                    a.[UserId],
                    a.[Region],
                    a.[Country],
                    a.[ApplicationType] AS AppType,
                    a.[Country] AS ApplicationCountry,
                    a.[Site] AS ApplicationSite,
                    a.[Schedule] AS ApplicationSchedule,
                    a.[isCourtesyLane] AS ApplicationCourtesy,
                    a.[isPaid] AS ApplicationPaid,
                    a.[CitizenshipBasis] AS ApplicationCitizenshipBasis,
                    a.[ApplicationStatus],
                    a.[ApplicationBarCodePath],
                    a.[Amount],
                    a.[ValidIdPath],
                    a.[CertificatePath],

                    ROW_NUMBER() OVER (
                        PARTITION BY e.ApplicationCode 
                        ORDER BY e.Id DESC
                    ) AS rn

                FROM [EnrollmentRegistryID] e

                 JOIN [Applications] a
                    ON e.[ApplicationCode] = a.[ApplicationCode]

                 JOIN [PassportPersonalInformation] p
                    ON p.[PassportPersonalInformationId] = a.[PassportPersonalInformationId]
            )

            SELECT *
            FROM Ranked
            WHERE rn = 1
        ")
                .ToListAsync();

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
