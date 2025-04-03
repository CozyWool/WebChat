using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<List<UserEntity?>> GetAll() =>
        await dbContext.Users.ToListAsync();

    public async Task<UserEntity?> GetById(Guid id) =>
        await dbContext
            .Users
            .Include(e => e.Role)
            .FirstOrDefaultAsync(x => x.Id == id);
    public async Task<UserEntity?> GetByEmailOrUsername(string emailOrUsername)
    {
        return await dbContext
            .Users
            .Include(e => e.Role)
            .FirstOrDefaultAsync(x => x.Username == emailOrUsername || x.Email == emailOrUsername);
    }  public async Task<UserEntity?> GetByEmailOrUsername(string email, string username)
    {
        return await dbContext
            .Users
            .Include(e => e.Role)
            .FirstOrDefaultAsync(x => x.Username == username || x.Email == email);
    }
    
    public async Task Create(UserEntity? entity)
    {
        if (entity is null)
            return;

        if (entity.RoleId == 0)
        {
            var userRoleId = dbContext.Roles.FirstOrDefault(x => x.Name == "User").Id;
            entity.RoleId = userRoleId;
        }

        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await GetById(id);
        if (entity == null) return false;
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
        dbContext.Update(entity);
        await dbContext.SaveChangesAsync();
    }
}