using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Repositories;

public interface IUserRepository
{
    Task<List<UserEntity>> GetAll();
    Task<UserEntity?> GetById(Guid id);
    Task<UserEntity?> GetByEmailOrUsername(string emailOrUsername);
    Task<UserEntity?> GetByEmailOrUsername(string email, string username);

    Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFiltered(int pageNumber,
                                                                          int pageSize,
                                                                          UserSortState sortOrder,
                                                                          string? username,
                                                                          UserRelationTypes? relationType,
                                                                          int? role,
                                                                          string? currentUsername);

    Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFilteredByMultipleRelationTypes(
        int pageNumber,
        int pageSize,
        List<UserRelationTypes> relationTypes,
        string? currentUsername,
        string? username,
        int? roleId,
        UserSortState sortOrder);

    Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFilteredByMultipleRoles(
        int pageNumber,
        int pageSize,
        List<int> roles,
        string? currentUsername,
        string? username,
        UserSortState sortOrder);

    Task Create(UserEntity? entity);
    Task Update(UserEntity entity);
    Task<bool> Delete(Guid id);
    Task<bool> Delete(string username);
}