using System.Globalization;
using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Services;

namespace WebChatApplication.Profiles;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<MessageEntity, MessageModel>().AfterMap<MessageModelMappingAction>().ReverseMap();
    }

    private class MessageModelMappingAction(ICurrentUserService currentUserService, IUserService userService)
        : IMappingAction<MessageEntity, MessageModel>
    {
        public void Process(MessageEntity source, MessageModel destination, ResolutionContext context)
        {
            destination.IsCurrentUserSentMessage = currentUserService.CurrentUserId == source.UserId;
            // Очень долго выполняется
            // destination.Author = context.Mapper.Map<UserCardModel>(source.User);
            destination.Author = new UserCardModel
                                 {
                                     Username = source.User.Username,
                                     ProfilePictureUrl = userService
                                                         .GetProfilePictureUrlByFilename(source.User.Username,
                                                          source.User.ProfilePictureFileName)
                                                         .Result
                                 };
        }
    }
}