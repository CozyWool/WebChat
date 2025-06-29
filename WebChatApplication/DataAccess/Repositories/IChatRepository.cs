using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IChatRepository
{
    Task<List<ChatEntity>> GetCurrentUserChats();
    Task<ChatEntity?> GetById(Guid id, int messageCount, int alreadyLoadedMessageCount);
    Task<ChatEntity?> Create(ChatEntity? entity);
    Task Update(ChatEntity entity);
}