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
    private readonly IMessageRepository _messageRepository;

    public ChatRepository(ApplicationDbContext dbContext, ICurrentUserService currentUserService, IMessageRepository messageRepository)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _messageRepository = messageRepository;
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

    public async Task<ChatEntity?> GetById(Guid id, int messageCount)
    {
        var chat = await GetChatsQueryable().FirstOrDefaultAsync(c => c.Id == id);
        // TODO
        // if (chat is not null)
        // {
        //     chat.Messages = await _messageRepository.GetByChatId(id, 50);
        // }
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

    private IQueryable<ChatEntity> GetChatsQueryable()
    {
        return _dbContext
               .Chats
               .Include(e => e.Owner)
               .Include(e => e.Users)
               .Include(e => e.Messages)
               .AsSplitQuery();
    }
}