using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.Services;

public interface IMessageService
{
    Task<bool> Delete(Guid id);
}