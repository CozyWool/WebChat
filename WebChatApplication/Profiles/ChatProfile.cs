using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Services;

namespace WebChatApplication.Profiles;

public class ChatProfile : Profile
{
    public ChatProfile()
    {
        CreateMap<ChatEntity, PrivateChatModel>().AfterMap<PrivateChatModelMappingAction>().ReverseMap();
    }

    private class PrivateChatModelMappingAction(ICurrentUserService currentUserService)
        : IMappingAction<ChatEntity, PrivateChatModel>
    {
        public void Process(ChatEntity source, PrivateChatModel destination, ResolutionContext context)
        {
            destination.CurrentUser = context.Mapper.Map<UserCardModel>(currentUserService.GetCurrentUser().Result);
            destination.PrivateUser = context
                                      .Mapper
                                      .Map<UserCardModel>
                                          (source
                                           .Users
                                           .FirstOrDefault(e => e.Id != currentUserService.CurrentUserId));
            destination.Messages =
                context.Mapper.Map<List<MessageModel>>(source.Messages.OrderBy(e => e.SentAt).ToList());
        }
    }
}