using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace WebChatApplication.Services;

public class S3Service : IS3Service
{
    private readonly IMinioClient _minioClient;

    public S3Service(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<string> GetUrl(string bucketId, string file)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketId)
            .WithObject(file)
            .WithExpiry((int) TimeSpan.FromHours(1).TotalSeconds);
        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task<string> UploadFile(string bucketId, string fileName, Stream fileStream)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}_{fileName}";
            const string contentType = "application/octet-stream";

            var args = new BucketExistsArgs().WithBucket(bucketId);
            var found = await _minioClient.BucketExistsAsync(args);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketId));
            }

            var putArgs = new PutObjectArgs()
                .WithBucket(bucketId)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putArgs);
            return objectName;
        }
        catch (MinioException ex)
        {
            return "";
        }
    }

    public async Task<bool> DeleteFile(string bucketId, string fileName)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(bucketId)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(args);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}