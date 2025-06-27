using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;

namespace WebChatApplication.Services;

public interface IChatService
{
    Task<ChatModel?> GetChatById(Guid chatId, int messageCount, int alreadyLoadedMessageCount);
    Task<PrivateChatModel?> GetPrivateChatByUserId(Guid userId);
    Task<GroupChatModel?> GetGroupChatByUserIds(List<Guid> userIds);
    Task<ChatEntity?> CreatePrivateChat(Guid userId);
    Task<ChatEntity?> CreateGroupChat(List<Guid> userIds);
    Task<WebChatViewModel> GetChatsByUsername(string? username, Guid? chatId, int messageCount = 50);
}