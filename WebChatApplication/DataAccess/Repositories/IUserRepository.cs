using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Repositories;

public interface IUserRepository
{
    Task<List<UserEntity>> GetAll();
    Task<UserEntity?> GetById(Guid id);
    Task<UserEntity?> GetByEmailOrUsername(string emailOrUsername);
    Task<UserEntity?> GetByEmailOrUsername(string email, string username);

    Task<(List<UserEntity> items, int count)> GetFriendsPagedSortedFiltered(int pageNumber, int pageSize,
                                                                     FriendSortState sortOrder, string? username,
                                                                     string? currentUsername);

    Task Create(UserEntity? entity);
    Task Update(UserEntity entity);
    Task<bool> Delete(Guid id);
    Task<bool> Delete(string username);
}