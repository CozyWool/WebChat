using AutoMapper;
using WebChatApplication.Configurations;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;
using WebChatApplication.Services;

namespace WebChatApplication.Profiles;

public class AttachmentProfile : Profile
{
    public AttachmentProfile()
    {
        CreateMap<AttachmentEntity, AttachmentModel>().AfterMap<AttachmentModelMappingAction>().ReverseMap();
    }
    private class AttachmentModelMappingAction(IS3Service s3Service, IConfiguration configuration)
        : IMappingAction<AttachmentEntity, AttachmentModel>
    {
        public void Process(AttachmentEntity source, AttachmentModel destination, ResolutionContext context)
        {
            var bucketId = configuration.GetSection("MinioConfiguration").Get<MinioConfiguration>().BucketId;
            destination.PathUrl = s3Service.GetUrl(bucketId, source.Path).Result;
        }
    }
}