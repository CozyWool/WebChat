using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
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
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public ChatHub(IChatService chatService,
                   ICurrentUserService currentUserService,
                   IMapper mapper,
                   ApplicationDbContext context)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _context = context;
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task SendMessage(Guid chatId, string messageJson)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         DateFormatString =
                                             "dd.MM.yyyy HH:mm:ss",
                                         DateTimeZoneHandling =
                                             DateTimeZoneHandling.Utc,
                                     };
        var messageModel = JsonConvert.DeserializeObject<MessageModel>(messageJson, jsonSerializerSettings);
        var chat = await _chatService.GetChatById(chatId, 0);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null || chat is null)
        {
            return;
        }

        var messageEntity = _mapper.Map<MessageEntity>(messageModel);
        messageEntity.ChatId = chatId;
        messageEntity.ParentMessageId = messageModel?.ParentMessage?.Id;
        messageEntity.UserId = currentUserId.Value;
        messageEntity.ParentMessage = null;
        messageEntity.User = null;

        await _context.Messages.AddAsync(messageEntity);
        await _context.SaveChangesAsync();

        await Clients
              .OthersInGroup(groupName: chatId.ToString())
              .SendAsync(method: "ReceiveMessage",
                         JsonConvert.SerializeObject(messageModel, jsonSerializerSettings));
    }

    public async Task DeleteMessage(Guid chatId, Guid messageId)
    {
        var chat = await _chatService.GetChatById(chatId, 0);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null || chat is null)
        {
            return;
        }
        
        await Clients
              .OthersInGroup(groupName: chatId.ToString())
              .SendAsync(method: "DeleteMessage",
                         messageId);
    }
}