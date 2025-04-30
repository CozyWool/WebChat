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
                     .ToListAsync();
    }

    public async Task<ChatEntity?> GetById(Guid id)
    {
        return await GetChatsQueryable().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Guid?> Create(ChatEntity? entity)
    {
        if (entity is null)
        {
            return null;
        }

        var chatId = _dbContext.Chats.Add(entity).Entity.Id;
        await _dbContext.SaveChangesAsync();
        return chatId;
    }

    private IIncludableQueryable<ChatEntity, ICollection<MessageEntity>> GetChatsQueryable()
    {
        return _dbContext
               .Chats
               .Include(e => e.Owner)
               .Include(e => e.Users)
               .Include(e => e.Messages);
    }
}