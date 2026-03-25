using Microsoft.AspNetCore.Mvc;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;

        private const string TempFolder = "temp_uploads";

        public ApplicationController(
            AppDbContext context,
            IWebHostEnvironment env,
            IFileService fileService)
        {
            _context = context;
            _env = env;
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromForm] CreateApplicationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string tempPath = Path.Combine(_env.WebRootPath, "temp_uploads");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            async Task<string> SaveTempFile(IFormFile file)
            {
                string ext = Path.GetExtension(file.FileName);
                string fileName = $"{Guid.NewGuid()}{ext}";
                string fullPath = Path.Combine(tempPath, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }

            // Save to temp
            string validIdKey = await SaveTempFile(dto.ValidId);
            string certificateKey = await SaveTempFile(dto.Certificate);

            // FINALIZE BEFORE DB SAVE
            string validIdPath = await _fileService.FinalizeUpload(validIdKey, dto.UserId);
            string certificatePath = await _fileService.FinalizeUpload(certificateKey, dto.UserId);

            // Now create entity with paths READY
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

            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }
    }
}
