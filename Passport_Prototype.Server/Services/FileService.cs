using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OnlineRegistration.Server.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRegistration.Server.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        private const string PermanentBaseFolder = "PermanentPDSFiles";
        private const string TemporaryBaseFolder = "temp_uploads";

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            string wwwroot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            string tempFolder = Path.Combine(wwwroot, TemporaryBaseFolder);
            Directory.CreateDirectory(tempFolder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string fullPath = Path.Combine(tempFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{TemporaryBaseFolder}/{fileName}";
        }

        public async Task<string> FinalizeUpload(string fileKey, int personId)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentException("File key cannot be null or empty.", nameof(fileKey));

            string tempFilePath = Path.Combine(_env.WebRootPath, TemporaryBaseFolder, fileKey);
            if (!File.Exists(tempFilePath))
                throw new FileNotFoundException($"Temp file not found: {fileKey}");

            string ext = Path.GetExtension(fileKey)?.ToLowerInvariant();
            string[] allowed = { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(ext))
                throw new NotSupportedException($"File type {ext} is not allowed.");

            string permanentFolder = Path.Combine(_env.WebRootPath, PermanentBaseFolder, personId.ToString());
            Directory.CreateDirectory(permanentFolder);

            string permanentFileName = $"{Guid.NewGuid()}{ext}";
            string permanentFilePath = Path.Combine(permanentFolder, permanentFileName);

            using var source = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
            using var dest = new FileStream(permanentFilePath, FileMode.Create, FileAccess.Write);
            await source.CopyToAsync(dest);

            // TEMP FILE IS NOT DELETED
            return Path.Combine("/", PermanentBaseFolder, personId.ToString(), permanentFileName).Replace('\\', '/');
        }

        public Task<bool> DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return Task.FromResult(true);

            try
            {
                string fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
                return Task.FromResult(true);
            }
            catch { return Task.FromResult(false); }
        }

        public bool DeleteTempFile(string fileName)
        {
            string tempFilePath = Path.Combine(_env.WebRootPath, TemporaryBaseFolder, fileName);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
                return true;
            }
            return false;
        }
    }
}