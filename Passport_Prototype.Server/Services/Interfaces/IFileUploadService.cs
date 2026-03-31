using Microsoft.AspNetCore.Http;

namespace Passport_Prototype.Server.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file);
        bool DeleteFile(string fileName);
    }
}