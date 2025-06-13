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

    public async Task<ChatModel?> GetChatById(Guid chatId, int messageCount = 50)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var chatEntity = await _chatRepository.GetById(chatId, messageCount);
        if (currentUser is null || chatEntity is null)
        {
            return null;
        }

        var chatModel = _mapper.Map<ChatModel>(chatEntity);
        switch (chatModel.ChatType)
        {
            case ChatTypes.Private:
            {
                var privateModel = _mapper.Map<PrivateChatModel>(chatModel);
                return privateModel;
            }
            case ChatTypes.Group:
            {
                var groupChatModel = _mapper.Map<GroupChatModel>(chatModel);
                return groupChatModel;
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

        var relationToUser = currentUser.RelatedUsers.FirstOrDefault(x => x.ToUserId == userId);
        if (relationToUser?.RelationType is not UserRelationTypes.Friend)
        {
            return null;
        }

        var chatEntity = currentUser.Chats.FirstOrDefault(x =>
                                                          {
                                                              if (x.Users.Count != 2 || x.ChatType != ChatTypes.Private)
                                                              {
                                                                  return false;
                                                              }

                                                              return x.Users.FirstOrDefault(u => u.Id == userId) is not
                                                                         null;
                                                          });
        if (chatEntity is null)
        {
            chatEntity = await CreatePrivateChat(userId);
            if (chatEntity is null)
            {
                return null;
            }
        }

        var chatModel = _mapper.Map<ChatModel>(chatEntity);
        var privateChatModel = _mapper.Map<PrivateChatModel>(chatModel);
        return privateChatModel;
    }

    public async Task<ChatEntity?> CreatePrivateChat(Guid userId)
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
        var chat = await _chatRepository.Create(chatEntity);
        return chat;
    }

    public async Task<Guid?> CreateGroupChat(List<Guid> userIds)
    {
        throw new NotImplementedException();
    }

    public async Task<WebChatViewModel> GetChatsByUsername(string? username, int chatIndex)
    {
        var chats = await _chatRepository.GetCurrentUserChats();
        var mappedChats = _mapper.Map<List<ChatModel>>(chats);
        for (var i = 0; i < mappedChats.Count; i++)
        {
            switch (mappedChats[i].ChatType)
            {
                case ChatTypes.Private:
                    var privateChatModel = _mapper.Map<PrivateChatModel>(mappedChats[i]);
                    var currentUser = await _currentUserService.GetCurrentUser();
                    if (currentUser is not null)
                    {
                        var relationToUser =
                            currentUser.RelatedUsers.FirstOrDefault(x => x.ToUserId ==
                                                                         privateChatModel.PrivateUser.UserId);
                        if (relationToUser?.RelationType is not UserRelationTypes.Friend)
                        {
                            privateChatModel = null;
                        }
                    }


                    if (privateChatModel is null)
                    {
                        mappedChats.RemoveAt(i);
                        i--;
                        break;
                    }

                    mappedChats[i] = privateChatModel;
                    break;
            }
        }


        var model = new WebChatViewModel
                    {
                        CurrentChat = chatIndex < mappedChats.Count ? mappedChats[chatIndex] : null,
                        Chats = mappedChats
                    };
        return model;
    }
}