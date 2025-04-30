using AutoMapper;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Enums;
using WebChatApplication.Models;

namespace WebChatApplication.Services;

public class ChatService : IChatService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IChatRepository _chatRepository;

    public ChatService(IMapper mapper, IChatRepository chatRepository, ICurrentUserService currentUserService,
                       IUserRepository userRepository)
    {
        _mapper = mapper;
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<ChatModel?> GetChatById(Guid chatId)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var chat = await _chatRepository.GetById(chatId);
        if (currentUser is null || chat is null)
        {
            return null;
        }

        switch (chat.ChatType)
        {
            case ChatTypes.Private:
            {
                var model = _mapper.Map<PrivateChatModel>(chat);
                return model;
            }
            case ChatTypes.Group:
            {
                var model = _mapper.Map<GroupChatModel>(chat);
                return model;
            }
        }

        return null;
    }

    public async Task<PrivateChatModel?> GetPrivateChatByUserId(Guid userId)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return null;
        }

        var chatId = currentUser.Chats.FirstOrDefault(x => x.Users.Any(u => u.Id == userId))?.Id;
        if (chatId is null)
        {
            chatId = await CreatePrivateChat(userId);
            if (chatId is null)
            {
                return null;
            }
        }

        var chat = await _chatRepository.GetById(chatId.Value);
        if (chat is null)
        {
            return null;
        }

        var model = _mapper.Map<PrivateChatModel>(chat);
        return model;
    }

    public async Task<Guid?> CreatePrivateChat(Guid userId)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var user = await _userRepository.GetById(userId);
        if (user is null || currentUser is null)
        {
            return null;
        }

        var chatEntity = new ChatEntity
                         {
                             Name = user.Username,
                             ChatType = ChatTypes.Private,
                             CreatedAt = DateTime.UtcNow,
                             Users = [currentUser, user],
                         };
        var chatId = await _chatRepository.Create(chatEntity);
        return chatId;
    }

    public async Task<Guid?> CreateGroupChat(List<Guid> userIds)
    {
        throw new NotImplementedException();
    }
}