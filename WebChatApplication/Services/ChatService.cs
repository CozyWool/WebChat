using AutoMapper;
using Newtonsoft.Json;
using WebChatApplication.Configurations;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Enums;
using WebChatApplication.Messages;
using WebChatApplication.Models;
using WebChatApplication.Models.User;

namespace WebChatApplication.Services;

public class ChatService : IChatService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IS3Service _s3Service;
    private readonly IMapper _mapper;
    private readonly IChatRepository _chatRepository;
    private readonly string _bucketId;

    public ChatService(IMapper mapper,
                       IChatRepository chatRepository,
                       ICurrentUserService currentUserService,
                       IUserRepository userRepository,
                       IConfiguration configuration,
                       IS3Service s3Service)
    {
        _mapper = mapper;
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _configuration = configuration;
        _s3Service = s3Service;
        _bucketId = _configuration.GetSection("MinioConfiguration").Get<MinioConfiguration>().BucketId;
    }

    public async Task<ChatModel?> GetChatById(Guid chatId, int messageCount = 50, int alreadyLoadedMessageCount = 0)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var chatEntity = await _chatRepository.GetById(chatId, messageCount, alreadyLoadedMessageCount);
        if (currentUser is null || chatEntity is null)
        {
            return null;
        }

        var chatModel = _mapper.Map<ChatModel>(chatEntity);
        chatModel.CurrentUser = _mapper.Map<UserCardModel>(currentUser);
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
        chatModel.CurrentUser = _mapper.Map<UserCardModel>(currentUser);
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

    public async Task<GroupChatModel?> GetGroupChatByUserIds(List<Guid> userIds)
    {
        throw new NotImplementedException();
    }

    public async Task<ChatEntity?> CreateGroupChat(GroupChatInfoRequest request)
    {
        var userIds = JsonConvert.DeserializeObject<List<Guid>>(request.UserIdsJson);
        var currentUser = await _currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return null;
        }

        userIds.Add(currentUser.Id);

        var users = await _userRepository.GetByIds(userIds);
        if (users.Count != userIds.Count)
        {
            return null;
        }

        var chatEntity = new ChatEntity
                         {
                             Name = request.ChatName,
                             ChatType = ChatTypes.Group,
                             CreatedAt = DateTime.UtcNow,
                             Users = users,
                             Owner = currentUser,
                         };
        var chat = await _chatRepository.Create(chatEntity);
        if (request.ChatPicture is not null)
        {
            var fileName = $"{chat.Id}_chatPfp{Path.GetExtension(request.ChatPicture.FileName)}";
            var chatPictureFileName = await _s3Service.UploadFile(_bucketId,
                                                                  fileName,
                                                                  request.ChatPicture.OpenReadStream());
            chat.ChatPictureFileName = chatPictureFileName;
            await _chatRepository.Update(chat);
        }

        return chat;
    }

    public async Task<WebChatViewModel> GetChatsByUsername(string? username, Guid? chatId, int messageCount = 50)
    {
        var chats = await _chatRepository.GetCurrentUserChats();
        var mappedChats = _mapper.Map<List<ChatModel>>(chats);
        var currentUser = await _currentUserService.GetCurrentUser();
        for (var i = 0; i < mappedChats.Count; i++)
        {
            mappedChats[i].CurrentUser = _mapper.Map<UserCardModel>(currentUser);
            switch (mappedChats[i].ChatType)
            {
                case ChatTypes.Private:
                    var privateChatModel = _mapper.Map<PrivateChatModel>(mappedChats[i]);
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
                case ChatTypes.Group:
                    var groupChatModel = _mapper.Map<GroupChatModel>(mappedChats[i]);
                    if (groupChatModel is null)
                    {
                        mappedChats.RemoveAt(i);
                        i--;
                        break;
                    }

                    var userIds = groupChatModel
                                  .Users
                                  .Select(x => x.UserId)
                                  .ToList();
                    var currentUserFriends = currentUser
                                             .RelatedUsers
                                             .Where(x => x.RelationType == UserRelationTypes.Friend)
                                             .Select(x => x.ToUser)
                                             .Where(x => !userIds.Contains(x.Id))
                                             .ToList();


                    groupChatModel.Friends = _mapper.Map<List<UserCardModel>>(currentUserFriends);
                    mappedChats[i] = groupChatModel;
                    break;
            }

            mappedChats[i].Messages = mappedChats[i].Messages.TakeLast(messageCount).ToList();
        }

        var currentChat = mappedChats.FirstOrDefault(x => x.Id == chatId);

        var model = new WebChatViewModel
                    {
                        CurrentChat = currentChat,
                        Chats = mappedChats,
                        Friends = await _currentUserService.GetFriends(),
                    };
        return model;
    }

    public async Task<string> GetChatPictureUrl(Guid chatId)
    {
        // TODO: тут лишний запрос в БД
        var chat = await _chatRepository.GetById(chatId, 0, 0);
        if (chat is null)
        {
            return "";
        }

        var url = await _s3Service.GetUrl(_bucketId, chat.ChatPictureFileName);
        if (string.IsNullOrEmpty(url))
        {
            url = $"https://ui-avatars.com/api/?name={chat.Name}&size=200p";
        }

        return url;
    }

    public async Task<bool> UpdateGroupChat(GroupChatInfoRequest request)
    {
        var chat = await _chatRepository.GetById(request.ChatId, 0, 0);
        if (chat is null || chat.ChatType != ChatTypes.Group)
        {
            return false;
        }

        chat.Name = request.ChatName;
        if (request.ChatPicture is not null)
        {
            var fileName = $"{chat.Id}_chatPfp{Path.GetExtension(request.ChatPicture.FileName)}";
            var chatPictureFileName = await _s3Service.UploadFile(_bucketId,
                                                                  fileName,
                                                                  request.ChatPicture.OpenReadStream());
            if (!string.IsNullOrEmpty(chatPictureFileName) &&
                (chat.ChatPictureFileName is null ||
                 await _s3Service.DeleteFile(_bucketId, chat.ChatPictureFileName)))
            {
                chat.ChatPictureFileName = chatPictureFileName;
            }
        }

        await _chatRepository.Update(chat);
        return true;
    }

    public async Task<bool> AddUsersToGroupChat(Guid chatId, string userIdsJson)
    {
        var chat = await _chatRepository.GetById(chatId, 0, 0);
        if (chat is null || chat.ChatType != ChatTypes.Group)  
        {
            return false;
        }

        var userIds = JsonConvert.DeserializeObject<List<Guid>>(userIdsJson);

        var users = await _userRepository.GetByIds(userIds);
        if (users.Count != userIds.Count)
        {
            return false;
        }

        foreach (var user in users)
        {
            if (!chat.Users.Contains(user))
            {
                chat.Users.Add(user);
            }
        }

        await _chatRepository.Update(chat);
        return true;
    }

    public async Task<bool> DeleteUserFromGroupChat(Guid chatId, Guid userId)
    {
        var chat = await _chatRepository.GetById(chatId, 0, 0);
        if (chat is null || chat.ChatType != ChatTypes.Group)  
        {
            return false;
        }

        var user = await _userRepository.GetById(userId);
        if (user is null)
        {
            return false;
        }

        chat.Users.Remove(user);
        
        await _chatRepository.Update(chat);
        return true;
    }
}