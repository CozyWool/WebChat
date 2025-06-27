using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models.User;

namespace WebChatApplication.Services;

public interface ICurrentUserService
{
    Guid? CurrentUserId { get; }
    Task<UserEntity?> GetCurrentUser();
    Task<List<UserCardModel>> GetFriends();
}