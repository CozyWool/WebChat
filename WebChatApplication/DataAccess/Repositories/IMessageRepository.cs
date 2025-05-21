using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IMessageRepository
{
    Task<bool> Delete(Guid id);
    Task<bool> Delete(MessageEntity? entity);
    Task<MessageEntity?> GetById(Guid id);
    Task<List<MessageEntity>> GetByChatId(Guid chatId, int count);
}