using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.Services;

public interface ICurrentUserService
{
    Guid? CurrentUserId { get; }
    Task<UserEntity?> GetCurrentUser();
}