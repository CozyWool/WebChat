using Minio;
using WebChatApplication.Configurations;

namespace WebChatApplication.Extensions;

public static class MinioExtensions
{
    public static void AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        var minioConfiguration = configuration
            .GetSection("MinioConfiguration")
            .Get<MinioConfiguration>();
        if (minioConfiguration == null) throw new Exception("MinioConfiguration is null");
        var endpoint = minioConfiguration.Endpoint;
        var accessKey = minioConfiguration.AccessKey;
        var secretKey = minioConfiguration.SecretKey;

        services.AddMinio(client =>
        {
            client
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build();
        });
    }
}