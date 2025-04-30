using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserEntity>> GetAll()
    {
        return await GetUsersQueryable()
                   .ToListAsync();
    }

    private IIncludableQueryable<UserEntity, ICollection<UserActionEntity>> GetUsersQueryable()
    {
        return _dbContext
               .Users
               .Include(e => e.Role)
               .Include(e => e.RelatedUsers).ThenInclude(e => e.ToUser)
               .Include(e => e.Messages)
               .Include(e => e.Chats).ThenInclude(e => e.Users)
               .Include(e => e.Actions);
    }

    public async Task<UserEntity?> GetById(Guid id)
    {
        return await GetUsersQueryable()
                   .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UserEntity?> GetByEmailOrUsername(string emailOrUsername)
    {
        return await GetUsersQueryable()
                   .FirstOrDefaultAsync(x => x.Username == emailOrUsername || x.Email == emailOrUsername);
    }

    public async Task<UserEntity?> GetByEmailOrUsername(string email, string username)
    {
        return await GetUsersQueryable()
                   .FirstOrDefaultAsync(x => x.Username == username || x.Email == email);
    }

    public async Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFiltered(int pageNumber,
        int pageSize,
        UserSortState sortOrder,
        string? username,
        UserRelationTypes? relationType,
        int? role,
        string? currentUsername)
    {
        //фильтрация
        var usersQuery = GetUsersQueryable().AsQueryable();

        usersQuery = FilterUsersByUsername(username, usersQuery);

        if (!string.IsNullOrEmpty(currentUsername))
        {
            usersQuery = usersQuery.Where(x => x.Username != currentUsername);

            if (relationType is not null)
            {
                var currentUser = await GetByEmailOrUsername(currentUsername);
                if (currentUser is not null)
                {
                    if (relationType is UserRelationTypes.NotRelated)
                    {
                        var relatedUsers = currentUser
                                           .RelatedUsers
                                           .Select(relation => relation.ToUser)
                                           .ToList();
                        usersQuery = usersQuery.Where(x => !relatedUsers.Contains(x));
                    }
                    else
                    {
                        var relatedUsers = currentUser
                                           .RelatedUsers
                                           .Where(x => x.RelationType == relationType)
                                           .Select(relation => relation.ToUser)
                                           .ToList();
                        usersQuery = usersQuery.Where(x => relatedUsers.Contains(x));
                    }
                }
            }
        }

        if (role is not null)
        {
            usersQuery = usersQuery.Where(x => x.Role.Id == role);
        }


        usersQuery = SortUsers(sortOrder, usersQuery);

        (usersQuery, var count) = await PaginateUsers(pageNumber, pageSize, usersQuery);
        var items = await usersQuery.ToListAsync();

        return (items, count);
    }

    private static IQueryable<UserEntity> FilterUsersByUsername(string? username, IQueryable<UserEntity> usersQuery)
    {
        if (!string.IsNullOrEmpty(username))
        {
            usersQuery = usersQuery.Where(x =>
                                              x.Username
                                               .ToLower()
                                               .Trim()
                                               .Contains(
                                                         username
                                                             .ToLower()
                                                             .Trim()));
        }

        return usersQuery;
    }

    private static IQueryable<UserEntity> SortUsers(UserSortState sortOrder, IQueryable<UserEntity> usersQuery)
    {
        usersQuery = sortOrder switch
                     {
                         UserSortState.UsernameAsc      => usersQuery.OrderBy(e => e.Username),
                         UserSortState.UsernameDesc     => usersQuery.OrderByDescending(e => e.Username),
                         UserSortState.LastActivityAsc  => usersQuery.OrderBy(e => e.LastActivityAt),
                         UserSortState.LastActivityDesc => usersQuery.OrderByDescending(e => e.LastActivityAt),
                     };
        return usersQuery;
    }

    private async Task<(IQueryable<UserEntity> items, int count)> PaginateUsers(
        int pageNumber, int pageSize, IQueryable<UserEntity> usersQuery)
    {
        var count = await usersQuery.CountAsync();

        usersQuery = usersQuery
                     .Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize);
        return (usersQuery, count);
    }

    public async Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFilteredByMultipleRelationTypes(
        int pageNumber,
        int pageSize,
        List<UserRelationTypes> relationTypes,
        string? currentUsername,
        string? username,
        UserSortState sortOrder)
    {
        var usersQuery = GetUsersQueryable().AsQueryable();

        usersQuery = FilterUsersByUsername(username, usersQuery);
        if (!string.IsNullOrEmpty(currentUsername))
        {
            usersQuery = usersQuery.Where(x => x.Username != currentUsername);

            var currentUser = await GetByEmailOrUsername(currentUsername);
            if (currentUser is not null && relationTypes.Count > 0)
            {
                var relatedUsers = currentUser
                                   .RelatedUsers
                                   .Where(x => relationTypes.Contains(x.RelationType))
                                   .Select(relation => relation.ToUser)
                                   .ToList();
                usersQuery = usersQuery.Where(x => relatedUsers.Contains(x));
            }
        }

        usersQuery = SortUsers(sortOrder, usersQuery);

        (usersQuery, var count) = await PaginateUsers(pageNumber, pageSize, usersQuery);

        var items = await usersQuery.ToListAsync();
        return (items, count);
    }

    public async Task<(List<UserEntity> items, int count)> GetUsersPagedSortedFilteredByMultipleRoles(
        int pageNumber,
        int pageSize,
        List<int> roles,
        string? currentUsername,
        string? username,
        UserSortState sortOrder)
    {
        var usersQuery = GetUsersQueryable().AsQueryable();

        if (!string.IsNullOrEmpty(currentUsername))
        {
            usersQuery = usersQuery.Where(x => x.Username != currentUsername);
        }

        usersQuery = FilterUsersByUsername(username, usersQuery);

        if (roles.Count > 0)
        {
            usersQuery = usersQuery.Where(x => roles.Contains(x.Role.Id));
        }

        usersQuery = SortUsers(sortOrder, usersQuery);

        (usersQuery, var count) = await PaginateUsers(pageNumber, pageSize, usersQuery);

        var items = await usersQuery.ToListAsync();
        return (items, count);
    }


    public async Task Create(UserEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        if (entity.RoleId == 0)
        {
            var userRoleId = _dbContext.Roles.FirstOrDefault(x => x.Name == "User").Id;
            entity.RoleId = userRoleId;
        }

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await GetById(id);
        if (entity is null)
        {
            return false;
        }

        entity.Status = UserStatuses.Deleted;
        await Update(entity);
        return true;
    }

    public async Task<bool> Delete(string username)
    {
        var entity = await GetByEmailOrUsername(username);
        return await Delete(entity.Id);
    }

    public async Task Update(UserEntity entity)
    {
        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();
    }
}