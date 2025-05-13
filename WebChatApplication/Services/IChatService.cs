using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;

namespace WebChatApplication.Services;

public interface IChatService
{
    Task<ChatModel?> GetChatById(Guid chatId);
    Task<PrivateChatModel?> GetPrivateChatByUserId(Guid userId);
    Task<ChatEntity?> CreatePrivateChat(Guid userId);
    Task<Guid?> CreateGroupChat(List<Guid> userIds);
}
