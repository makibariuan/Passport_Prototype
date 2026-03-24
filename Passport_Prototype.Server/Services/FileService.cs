using OnlineRegistration.Server.Services.Interfaces;
using System.IO;

namespace OnlineRegistration.Server.Services
{
    /// <summary>
    /// Concrete implementation of the IFileService for handling file uploads 
    /// and finalization logic.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        // Configuration values (e.g., from appsettings.json or DI)
        private const string PermanentBaseFolder = "PermanentPDSFiles";
        private const string TemporaryBaseFolder = "temp_uploads";

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Simulates the process of moving a file from temporary staging 
        /// to permanent storage.
        /// </summary>
        /// <param name="fileKey">The temporary filename/key.</param>
        /// <param name="entityId">The ID of the person the file belongs to.</param>
        /// <returns>The permanent relative path to the file.</returns>
        public async Task<string> FinalizeUpload(string fileKey, int entityId)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
            {
                throw new ArgumentException("File key cannot be null or empty.", nameof(fileKey));
            }

            // 1. Construct temporary and permanent file paths
            // ⭐ CRITICAL FIX: Use WebRootPath here to match the path used by the controller for saving.
            string tempDirectory = Path.Combine(_env.WebRootPath, TemporaryBaseFolder);
            string tempFilePath = Path.Combine(tempDirectory, fileKey);

            // Extract the original file extension (must be lower case for reliable comparison)
            string fileExtension = Path.GetExtension(fileKey)?.ToLowerInvariant();

            // ---------------------------------------------------------------------------------
            // ✅ NEW VALIDATION: Check Allowed Extensions
            // ---------------------------------------------------------------------------------
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            if (string.IsNullOrWhiteSpace(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                // Add logging here to track disallowed file types
                throw new NotSupportedException($"File type '{fileExtension}' is not supported. Only PDF, JPG, and PNG files are allowed.");
            }
            // ---------------------------------------------------------------------------------

            // Generate a secure permanent filename (using a GUID)
            string permanentFilename = $"{Guid.NewGuid()}{fileExtension}";

            // Create the user-specific permanent directory (e.g., PermanentPDSFiles/123)
            string permanentSubFolder = Path.Combine(PermanentBaseFolder, entityId.ToString());
            string permanentDirectory = Path.Combine(_env.WebRootPath, permanentSubFolder);

            // Ensure the permanent directory exists
            if (!Directory.Exists(permanentDirectory))
            {
                Directory.CreateDirectory(permanentDirectory);
            }

            string permanentFilePath = Path.Combine(permanentDirectory, permanentFilename);

            if (!File.Exists(tempFilePath))
            {
                throw new FileNotFoundException($"Temporary file not found for key: {fileKey}", tempFilePath);
            }

            try
            {
                // ⭐ NEW ASYNCHRONOUS FILE COPY OPERATION ⭐
                // 1. Open streams
                using (var sourceStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                using (var destinationStream = new FileStream(permanentFilePath, FileMode.Create, FileAccess.Write))
                {
                    // 2. Perform the copy asynchronously
                    await sourceStream.CopyToAsync(destinationStream);
                }

                // 3. Delete the temporary file synchronously (since the copy is complete)
                File.Delete(tempFilePath);

                // 4. Return the *relative* path that will be stored in the database
                string relativePathForDb = Path.Combine("/", permanentSubFolder, permanentFilename).Replace('\\', '/');

                return relativePathForDb; // Return string directly since method is async Task<string>
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException($"Error finalizing file {fileKey}.", ex);
            }
        }


        public Task<bool> DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(true); // Nothing to delete
            }

            try
            {
                // Convert the relative path stored in the database back to a physical path
                // Note: We are removing the leading '/' that was added for URL friendliness.
                string relativePath = filePath.TrimStart('/');
                string fullPath = Path.Combine(_env.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    // Log the deletion action here
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., failed permissions)
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {filePath}. Details: {ex.Message}");
                return Task.FromResult(false);
            }
        }
    }
}