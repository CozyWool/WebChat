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