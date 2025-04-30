using System.Globalization;
using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;
using WebChatApplication.Services;

namespace WebChatApplication.Profiles;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<MessageEntity, MessageModel>().AfterMap<MessageModelMappingAction>().ReverseMap();
    }

    private class MessageModelMappingAction(ICurrentUserService currentUserService)
        : IMappingAction<MessageEntity, MessageModel>
    {
        public void Process(MessageEntity source, MessageModel destination, ResolutionContext context)
        {
            destination.IsCurrentUserSentMessage = currentUserService.CurrentUserId == source.UserId;
        }
    }
}