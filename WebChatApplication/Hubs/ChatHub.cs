using Microsoft.AspNetCore.SignalR;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models;
using WebChatApplication.Models.User;
using WebChatApplication.Services;

namespace WebChatApplication.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public ChatHub(IChatService chatService,
                   ICurrentUserService currentUserService,
                   ApplicationDbContext context)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task SendMessage(Guid chatId, string content)
    {
        var sentAt = DateTime.UtcNow;

        var chat = await _chatService.GetChatById(chatId);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null || chat is null)
        {
            return;
        }
        var currentUserCard = chat.CurrentUser;
        
        List<string> userIds = [];
        switch (chat)
        {
            case PrivateChatModel privateChat:
                userIds = [privateChat.PrivateUser.UserId.ToString()];
                break;
            case GroupChatModel groupChat:
                userIds = groupChat
                          .Users
                          .Where(x => x.Id != currentUserId.Value)
                          .Select(x => x.Id.ToString())
                          .ToList();
                break;
        }

        var messageEntity = new MessageEntity
                            {
                                UserId = currentUserId.Value,
                                ChatId = chatId,
                                SentAt = sentAt,
                                Content = content,
                            };
        await _context.Messages.AddAsync(messageEntity);
        await _context.SaveChangesAsync();
        await Clients.Users(userIds).SendAsync("ReceiveMessage",
                                               content, 
                                               sentAt.ToString(), 
                                               currentUserCard);
    }
}