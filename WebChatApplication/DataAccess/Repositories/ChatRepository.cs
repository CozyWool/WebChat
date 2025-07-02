using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Services;

namespace WebChatApplication.DataAccess.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ChatRepository(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<ChatEntity>> GetCurrentUserChats()
    {
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null)
        {
            return [];
        }

        return await GetChatsQueryable()
                     .Where(c => c
                                 .Users
                                 .Any(u => u.Id == currentUserId))
                     .OrderByDescending(x => x.Messages.Count > 0
                                                 ? x.Messages.OrderBy(e => e.SentAt).Last().SentAt
                                                 : DateTime.MinValue)
                     .ToListAsync();
    }

    public async Task<ChatEntity?> GetById(Guid id, int messageCount, int alreadyLoadedMessageCount)
    {
        var chat = await GetChatsQueryable().FirstOrDefaultAsync(c => c.Id == id);

        if (chat is not null && messageCount > 0)
        {
            chat.Messages = chat
                            .Messages
                            .OrderBy(m => m.SentAt)
                            .SkipLast(alreadyLoadedMessageCount)
                            .TakeLast(messageCount)
                            .ToList();
        }

        return chat;
    }

    public async Task<ChatEntity?> Create(ChatEntity? entity)
    {
        if (entity is null)
        {
            return null;
        }

        var chat = _dbContext.Chats.Add(entity).Entity;
        await _dbContext.SaveChangesAsync();
        return chat;
    }

    public async Task Update(ChatEntity entity)
    {
        var oldEntity = await GetById(entity.Id, 0, 0);
        if (oldEntity is null)
        {
            return;
        }

        oldEntity.Name = entity.Name;
        oldEntity.ChatPictureFileName = entity.ChatPictureFileName;
        
        _dbContext.Chats.Update(oldEntity);
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<ChatEntity> GetChatsQueryable()
    {
        return _dbContext
               .Chats
               .Include(e => e.Owner)
               .Include(e => e.Users)
               .ThenInclude(e => e.RelatedUsers)
               .ThenInclude(e => e.ToUser)
               .Include(e => e.Messages)
               .ThenInclude(e => e.User)
               .AsSplitQuery();
    }
}