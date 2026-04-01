//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace Passport_Prototype.Server.Services
//{
//    public class FileUploadService : IFileUploadService
//    {
//        private readonly IWebHostEnvironment _environment;
//        private const string FolderName = "temp_uploads";

//        public FileUploadService(IWebHostEnvironment environment)
//        {
//            _environment = environment;
//        }

//        public async Task<string> UploadFileAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("File is empty or null.");

//            string wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
//            string uploadPath = Path.Combine(wwwrootPath, FolderName);

//            if (!Directory.Exists(uploadPath))
//                Directory.CreateDirectory(uploadPath);

//            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
//            string fullPath = Path.Combine(uploadPath, fileName);

//            try
//            {
//                await using var stream = new FileStream(fullPath, FileMode.Create);
//                await file.CopyToAsync(stream);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"UPLOAD CRASH: {ex.Message}");
//                throw;
//            }

//            // Return relative URL for access via browser
//            return $"/{FolderName}/{fileName}";
//        }

//        /*
//        public async Task<string> UploadFileAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("File is empty or null.");

//            try
//            {
//                // 1. Get the absolute path to wwwroot
//                // This resolves to .../Passport_Prototype.Server/wwwroot
//                string wwwrootPath = _environment.WebRootPath;

//                // 2. If WebRootPath is null (common in some API templates), fallback to ContentRoot
//                if (string.IsNullOrEmpty(wwwrootPath))
//                {
//                    wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
//                }

//                // 3. Combine with your specific folder: wwwroot/temp_uploads
//                string finalPath = Path.Combine(wwwrootPath, FolderName);

//                // 4. Ensure the folder exists on the server disk
//                if (!Directory.Exists(finalPath))
//                {
//                    Directory.CreateDirectory(finalPath);
//                }

//                // 5. Generate unique filename to prevent overwriting
//                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
//                string fullPath = Path.Combine(finalPath, fileName);

//                // 6. Save the file
//                using (var stream = new FileStream(fullPath, FileMode.Create))
//                {
//                    await file.CopyToAsync(stream);
//                }

//                return fileName;
//            }
//            catch (Exception ex)
//            {
//                // Log the error so you can see why it's crashing in the Debug Output
//                Console.WriteLine($"UPLOAD CRASH: {ex.Message}");
//                throw;
//            }
//        }
//        */

//        public bool DeleteFile(string fileName)
//        {
//            string wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
//            string filePath = Path.Combine(wwwrootPath, FolderName, fileName);

//            if (File.Exists(filePath))
//            {
//                File.Delete(filePath);
//                return true;
//            }
//            return false;
//        }
//    }
//}