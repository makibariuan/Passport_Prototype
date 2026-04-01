using Microsoft.AspNetCore.Mvc;
using OnlineRegistration.Server.Services.Interfaces; // Make sure IFileService is in this namespace
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileUploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // POST: api/FileUpload/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
                return BadRequest("No file provided.");

            try
            {
                // Upload file to temp_uploads
                var filePath = await _fileService.UploadFileAsync(file);

                return Ok(new
                {
                    Message = "File uploaded successfully",
                    FilePath = filePath // Returns relative path like /temp_uploads/{guid}.ext
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        // DELETE: api/FileUpload/{fileName}
        [HttpDelete("{fileName}")]
        public IActionResult DeleteTemp(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("File name required.");

            var success = _fileService.DeleteTempFile(fileName);
            return success ? Ok("Temp file removed.") : NotFound("File not found in temp folder.");
        }
    }
}