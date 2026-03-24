public interface IFileService
{
    // The PersonID is often included to create a unique, segregated folder path (e.g., /documents/personid/...)
    Task<string> FinalizeUpload(string fileKey, int personId);
    Task<bool> DeleteFile(string filePath);
}
