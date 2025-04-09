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

    public async Task<string> GetUrl(string bucketId, string? file)
    {
        if (string.IsNullOrEmpty(file))
        {
            return "";
        }
        var args = new PresignedGetObjectArgs()
                   .WithBucket(bucketId)
                   .WithObject(file)
                   .WithExpiry((int) TimeSpan.FromHours(1).TotalSeconds);
        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task<string?> UploadFile(string bucketId, string fileName, Stream fileStream)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}_{fileName}";
            const string contentType = "application/octet-stream";

            var isExist = await IsBucketExist(bucketId);
            if (!isExist)
            {
                await CreateBucket(bucketId);
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
        catch
        {
            return "";
        }
    }

    public async Task<string> UploadFile(string bucketId, string filePath)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}_{Path.GetFileName(filePath)}";
            const string contentType = "application/octet-stream";

            var isExist = await IsBucketExist(bucketId);
            if (!isExist)
            {
                await CreateBucket(bucketId);
            }


            var putArgs = new PutObjectArgs()
                          .WithBucket(bucketId)
                          .WithObject(objectName)
                          .WithFileName(filePath)
                          .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putArgs);
            return objectName;
        }
        catch
        {
            return "";
        }
    }

    public async Task<bool> CreateBucket(string bucketId)
    {
        try
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketId);
            await _minioClient.MakeBucketAsync(makeBucketArgs);

            await UploadFile(bucketId, Directory.GetCurrentDirectory() + "/wwwroot/images/user_default_pfp.png");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsBucketExist(string bucketId)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketId);
        var isFound = await _minioClient.BucketExistsAsync(bucketExistsArgs);
        return isFound;
    }

    public async Task<bool> DeleteFile(string bucketId, string? fileName)
    {
        try
        {
            var args = new RemoveObjectArgs()
                       .WithBucket(bucketId)
                       .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(args);
            return true;
        }
        catch
        {
            return false;
        }
    }
}