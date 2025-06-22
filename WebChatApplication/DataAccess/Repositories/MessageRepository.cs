using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MessageRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Create(MessageEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        await _dbContext.Messages.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(MessageEntity entity)
    {
        var oldEntity = await GetById(entity.Id);
        if (oldEntity is null)
        {
            return;
        }
        oldEntity.Content = entity.Content;
        oldEntity.UpdatedAt = entity.UpdatedAt;
        _dbContext.Messages.Update(oldEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await GetById(id);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Messages.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(MessageEntity? entity)
    {
        if (entity is null)
        {
            return false;
        }

        _dbContext.Messages.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<MessageEntity?> GetById(Guid id)
    {
        return await _dbContext
              .Messages
              .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<MessageEntity>> GetByChatId(Guid chatId, int count)
    {
        throw new NotImplementedException();
    }
}