using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebChatApplication.Configurations;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Models;
using WebChatApplication.Services;

namespace WebChatApplication.Hubs;

public class FileUploadModel
{
    public string FileName { get; set; }
    public string Base64 { get; set; }
}

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IMessageRepository _messageRepository;
    private readonly IS3Service _s3Service;
    private readonly string _bucketId;
    private readonly IAttachmentRepository _attachmentRepository;

    public ChatHub(IChatService chatService,
                   ICurrentUserService currentUserService,
                   IMapper mapper,
                   IMessageRepository messageRepository,
                   IS3Service s3Service,
                   IConfiguration configuration,
                   IAttachmentRepository attachmentRepository)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _messageRepository = messageRepository;
        _s3Service = s3Service;
        _attachmentRepository = attachmentRepository;
        _bucketId = configuration.GetSection("MinioConfiguration").Get<MinioConfiguration>().BucketId;
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task SendMessage(Guid chatId, string messageJson, List<FileUploadModel> files)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         DateFormatString =
                                             "dd.MM.yyyy HH:mm:ss",
                                         DateTimeZoneHandling =
                                             DateTimeZoneHandling.Utc,
                                     };
        var messageModel = JsonConvert.DeserializeObject<MessageModel>(messageJson, jsonSerializerSettings);
        if (messageModel.Attachments is null)
        {
            messageModel.Attachments = [];
        }

        // var chat = await _chatService.GetChatById(chatId, 0, 0);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null /*|| chat is null*/)
        {
            return;
        }

        var messageEntity = _mapper.Map<MessageEntity>(messageModel);
        messageEntity.ChatId = chatId;
        messageEntity.ParentMessageId = messageModel?.ParentMessage?.Id;
        messageEntity.UserId = currentUserId.Value;
        messageEntity.ParentMessage = null;
        messageEntity.User = null;
        messageEntity.SentAt = DateTime.UtcNow;

        await _messageRepository.Create(messageEntity);
        foreach (var file in files)
        {
            var bytes = Convert.FromBase64String(file.Base64);
            using var stream = new MemoryStream(bytes);

            var attachmentId = Guid.NewGuid();
            var path = await _s3Service.UploadFile(_bucketId, $"{attachmentId}_{file.FileName}", stream);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var attachmentEntity = new AttachmentEntity
                                   {
                                       Id = attachmentId,
                                       Path = path,
                                       FileName = file.FileName,
                                       MessageId = messageEntity.Id,
                                   };
            await _attachmentRepository.Create(attachmentEntity);

            messageModel.Attachments.Add(_mapper.Map<AttachmentModel>(attachmentEntity));
        }

        await Clients
              .OthersInGroup(groupName: chatId.ToString())
              .SendAsync(method: "ReceiveMessage",
                         JsonConvert.SerializeObject(messageModel, jsonSerializerSettings));
    }

    public async Task EditMessage(Guid chatId, string messageJson)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         DateFormatString =
                                             "dd.MM.yyyy HH:mm:ss",
                                         DateTimeZoneHandling =
                                             DateTimeZoneHandling.Utc,
                                     };
        var messageModel = JsonConvert.DeserializeObject<MessageModel>(messageJson, jsonSerializerSettings);
        if (messageModel.Attachments is null)
        {
            messageModel.Attachments = [];
        }
        // var chat = await _chatService.GetChatById(chatId, 0, 0);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null /*|| chat is null*/)
        {
            return;
        }

        var messageEntity = _mapper.Map<MessageEntity>(messageModel);
        messageEntity.UpdatedAt = DateTime.UtcNow;
        var attachments = await _attachmentRepository.GetByMessageId(messageEntity.Id);
        foreach (var attachment in attachments)
        {
            messageModel.Attachments.Add(_mapper.Map<AttachmentModel>(attachment));
        }
        // await _attachmentRepository.DeleteAllByMessageId(messageEntity.Id);
        // foreach (var file in files)
        // {
        //     var bytes = Convert.FromBase64String(file.Base64);
        //     using var stream = new MemoryStream(bytes);
        //
        //     var attachmentId = Guid.NewGuid();
        //     var path = await _s3Service.UploadFile(_bucketId, $"{attachmentId}_{file.FileName}", stream);
        //     if (string.IsNullOrEmpty(path))
        //     {
        //         continue;
        //     }
        //
        //     var attachmentEntity = new AttachmentEntity
        //                            {
        //                                Id = attachmentId,
        //                                Path = path,
        //                                FileName = file.FileName,
        //                                MessageId = messageEntity.Id,
        //                            };
        //     await _attachmentRepository.Create(attachmentEntity);
        //
        //     messageModel.Attachments.Add(_mapper.Map<AttachmentModel>(attachmentEntity));
        // }

        await _messageRepository.Update(messageEntity);

        await Clients
              .OthersInGroup(groupName: chatId.ToString())
              .SendAsync(method: "ReceiveEditedMessage",
                         JsonConvert.SerializeObject(messageModel, jsonSerializerSettings));
    }

    public async Task DeleteMessage(Guid chatId, Guid messageId)
    {
        // var chat = await _chatService.GetChatById(chatId, 0, 0);
        var currentUserId = _currentUserService.CurrentUserId;
        if (currentUserId is null /*|| chat is null*/)
        {
            return;
        }

        await Clients
              .OthersInGroup(groupName: chatId.ToString())
              .SendAsync(method: "DeleteMessage",
                         messageId);
    }
}