using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using SeniorCitizen.Server.Models;

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
        public async Task<IActionResult> CreateApplication([FromForm] CreateApplicationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            string validIdPath = await _fileService.FinalizeUpload(validIdKey, dto.UserId);
            string certificatePath = await _fileService.FinalizeUpload(certificateKey, dto.UserId);

            // 2. Generate a Shared Application Code
            string sharedCode = GenerateApplicationCode();

            // 3. Create Passport Application Entity
            var application = new Application
            {
                UserId = dto.UserId,
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
                isPaid = false,
                ApplicationStatus = "Pending"
            };

            // 4. Create Registry Entry
            var registryEntry = new EnrollmentRegistryID
            {
                PersonID = dto.UserId,
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
                    IdDocumentIdNumber = app.IdDocumentIdNumber,
                    ValidIdPath = app.ValidIdPath,
                    CertificatePath = app.CertificatePath,
                    ProcessingType = app.ProcessingType,
                    PaymentMethod = app.PaymentMethod,
                    DeliveryOption = app.DeliveryOption,
                    isPaid = app.isPaid,
                    ApplicationStatus = app.ApplicationStatus,
                    // Add more application fields if needed
                }
            ).ToListAsync();

            return Ok(result);
        }

    }

}
