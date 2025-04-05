namespace WebChatApplication.Services;

public interface IS3Service
{
    Task<string> GetUrl(string bucketId, string file);
    Task<string> UploadFile(string bucketId, string fileName, Stream fileStream);
    Task<bool> DeleteFile(string bucketId, string fileName);
}