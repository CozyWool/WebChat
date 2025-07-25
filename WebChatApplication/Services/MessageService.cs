using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;

namespace WebChatApplication.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAttachmentRepository _attachmentRepository;

    public MessageService(IMessageRepository messageRepository,
                          ICurrentUserService currentUserService,
                          IAttachmentRepository attachmentRepository)
    {
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<bool> Delete(Guid id)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var messageEntity = await _messageRepository.GetById(id);
        if (currentUser?.Id != messageEntity?.UserId)
        {
            return false;
        }
        if (messageEntity?.Attachments.Count > 0)
        {
            var attachmentIds = messageEntity.Attachments.Select(x => x.Id).ToList();
            foreach (var attachmentId in attachmentIds)
            {
                await _attachmentRepository.Delete(attachmentId);
            }
        }
        return await _messageRepository.Delete(id);
    }
}