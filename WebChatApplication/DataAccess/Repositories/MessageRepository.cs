using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChatRepository _chatRepository;

    public MessageRepository(ApplicationDbContext dbContext, IChatRepository chatRepository)
    {
        _dbContext = dbContext;
        _chatRepository = chatRepository;
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
                     .Include(x => x.Attachments)
                     .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<MessageEntity>> GetByChatId(Guid chatId, int count)
    {
        var message = await _chatRepository.GetById(chatId, 0, 0);
        if (message is null)
        {
            return [];
        }

        return await _dbContext
                     .Messages
                     .Include(x => x.Attachments)
                     .Where(x => x.ChatId == chatId)
                     .ToListAsync();
    }
}