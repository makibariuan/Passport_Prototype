using Microsoft.AspNetCore.Mvc;
using Passport_Prototype.Server.Services;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var fileName = await _fileUploadService.UploadFileAsync(file);
                return Ok(new
                {
                    Message = "File uploaded successfully",
                    FileName = fileName
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{fileName}")]
        public IActionResult Delete(string fileName)
        {
            var success = _fileUploadService.DeleteFile(fileName);
            return success ? Ok("File removed.") : NotFound("File not found.");
        }
    }
}