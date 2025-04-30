using WebChatApplication.Models;

namespace WebChatApplication.Services;

public interface IChatService
{
    Task<ChatModel?> GetChatById(Guid chatId);
    Task<PrivateChatModel?> GetPrivateChatByUserId(Guid userId);
    Task<Guid?> CreatePrivateChat(Guid userId);
    Task<Guid?> CreateGroupChat(List<Guid> userIds);
}
