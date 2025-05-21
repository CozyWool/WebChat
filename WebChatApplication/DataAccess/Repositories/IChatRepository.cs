using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IChatRepository
{
    Task<List<ChatEntity>> GetCurrentUserChats();
    Task<ChatEntity?> GetById(Guid id, int messageCount = 50);
    Task<ChatEntity?> Create(ChatEntity? entity);
}