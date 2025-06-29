using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Messages;
using WebChatApplication.Models;

namespace WebChatApplication.Services;

public interface IChatService
{
    Task<ChatModel?> GetChatById(Guid chatId, int messageCount, int alreadyLoadedMessageCount);
    Task<PrivateChatModel?> GetPrivateChatByUserId(Guid userId);
    Task<GroupChatModel?> GetGroupChatByUserIds(List<Guid> userIds);
    Task<ChatEntity?> CreatePrivateChat(Guid userId);
    Task<ChatEntity?> CreateGroupChat(GroupChatInfoRequest request);
    Task<WebChatViewModel> GetChatsByUsername(string? username, Guid? chatId, int messageCount = 50);
    Task<string> GetChatPictureUrl(Guid filename);
    Task<bool> UpdateGroupChat(GroupChatInfoRequest request);
}