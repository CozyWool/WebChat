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
               .Include(e => e.Chats)
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

    public async Task<(List<UserEntity> items, int count)> GetFriendsPagedSortedFiltered(
        int pageNumber, int pageSize, FriendSortState sortOrder, string? username, string? currentUsername)
    {
        //фильтрация
        var users = GetUsersQueryable().AsQueryable();

        if (!string.IsNullOrEmpty(username))
        {
            users = users.Where(x =>
                                    x.Username
                                     .ToLower()
                                     .Trim()
                                     .Contains(
                                               username
                                                   .ToLower()
                                                   .Trim()));
        }

        if (!string.IsNullOrEmpty(currentUsername))
        {
            users = users.Where(x => x.Username != currentUsername);
        }

        // сортировка
        users = sortOrder switch
                {
                    FriendSortState.UsernameAsc      => users.OrderBy(e => e.Username),
                    FriendSortState.UsernameDesc     => users.OrderByDescending(e => e.Username),
                    FriendSortState.LastActivityAsc  => users.OrderBy(e => e.LastActivityAt),
                    FriendSortState.LastActivityDesc => users.OrderByDescending(e => e.LastActivityAt),
                };


        // пагинация
        var count = await users.CountAsync();

        var items = await users
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
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