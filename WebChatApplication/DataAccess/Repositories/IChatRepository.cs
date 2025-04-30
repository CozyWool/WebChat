using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IChatRepository
{
    Task<List<ChatEntity>> GetCurrentUserChats();
    Task<ChatEntity?> GetById(Guid id);
    Task<Guid?> Create(ChatEntity? entity);
}