using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public class UserRelationRepository : IUserRelationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRelationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserRelationEntity>> GetAll()
    {
        return await _dbContext
                     .UserRelations
                     .Include(e => e.FromUser)
                     .Include(e => e.ToUser)
                     .ToListAsync();
    }

    public async Task<List<UserRelationEntity>> GetAllByFromId(Guid fromUserId)
    {
        return await _dbContext
                     .UserRelations
                     .Include(e => e.FromUser)
                     .Include(e => e.ToUser)
                     .Where(x => x.FromUserId == fromUserId)
                     .ToListAsync();
    }

    public async Task<List<UserRelationEntity>> GetAllByToId(Guid toUserId)
    {
        return await _dbContext
                     .UserRelations
                     .Include(e => e.FromUser)
                     .Include(e => e.ToUser)
                     .Where(x => x.ToUserId == toUserId)
                     .ToListAsync();
    }

    public async Task<UserRelationEntity?> GetById(Guid fromUserId, Guid toUserId)
    {
        return await _dbContext
                     .UserRelations
                     .Include(e => e.FromUser)
                     .Include(e => e.ToUser)
                     .FirstOrDefaultAsync(x => x.FromUserId == fromUserId && x.ToUserId == toUserId);
    }


    public async Task Create(UserRelationEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        _dbContext.UserRelations.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(UserRelationEntity entity)
    {
        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid fromId, Guid toId)
    {
        var entityFrom = await GetById(fromId, toId);
        var entityTo = await GetById(toId, fromId);
        if (entityFrom is null || entityTo is null)
        {
            return false;
        }

        _dbContext.UserRelations.Remove(entityFrom);
        _dbContext.UserRelations.Remove(entityTo);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}