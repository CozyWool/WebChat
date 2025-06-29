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
        CreateMap<ChatEntity, ChatModel>().AfterMap<ChatModelMappingAction>().ReverseMap();
        CreateMap<ChatModel, PrivateChatModel>().AfterMap<PrivateChatModelMappingAction>().ReverseMap();
        CreateMap<ChatModel, GroupChatModel>().AfterMap<GroupChatModelMappingAction>().ReverseMap();
    }

    private class ChatModelMappingAction(ICurrentUserService currentUserService)
        : IMappingAction<ChatEntity, ChatModel>
    {
        public void Process(ChatEntity source, ChatModel destination, ResolutionContext context)
        {
            destination.CurrentUser = context.Mapper.Map<UserCardModel>(currentUserService.GetCurrentUser().Result);
            destination.Messages =
                context.Mapper.Map<List<MessageModel>>(source.Messages.OrderBy(e => e.SentAt).ToList());
        }
    }

    private class PrivateChatModelMappingAction : IMappingAction<ChatModel, PrivateChatModel>
    {
        public void Process(ChatModel source, PrivateChatModel destination, ResolutionContext context)
        {
            destination.PrivateUser = source
                                      .Users
                                      .FirstOrDefault(e => e.Username != source.CurrentUser.Username)
                                      ?? throw new InvalidOperationException("Private user not found");
        }
    }
    private class GroupChatModelMappingAction(IChatService chatService) : IMappingAction<ChatModel, GroupChatModel>
    {
        public void Process(ChatModel source, GroupChatModel destination, ResolutionContext context)
        {
            destination.ChatPictureUrl = chatService.GetChatPictureUrl(source.Id).Result;
            destination.IsCurrentUserChatOwner = source.CurrentUser.UserId == source.Owner.UserId;
        }
    }
}