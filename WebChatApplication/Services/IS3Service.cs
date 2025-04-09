namespace WebChatApplication.Services;

public interface IS3Service
{
    Task<string> GetUrl(string bucketId, string? file);
    Task<string?> UploadFile(string bucketId, string fileName, Stream fileStream);
    Task<string> UploadFile(string bucketId, string filePath);
    Task<bool> DeleteFile(string bucketId, string? fileName);
    Task<bool> CreateBucket(string bucketId);
    Task<bool> IsBucketExist(string bucketId);
}