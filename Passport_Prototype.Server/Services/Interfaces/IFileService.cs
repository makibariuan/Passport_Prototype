using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OnlineRegistration.Server.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<string> FinalizeUpload(string fileKey, int personId);
        Task<bool> DeleteFile(string filePath);
        bool DeleteTempFile(string fileName);
    }
}