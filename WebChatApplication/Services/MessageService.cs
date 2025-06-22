using WebChatApplication.DataAccess.Entities;
using WebChatApplication.DataAccess.Repositories;

namespace WebChatApplication.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;

    public MessageService(IMessageRepository messageRepository, ICurrentUserService currentUserService)
    {
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Delete(Guid id)
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        var messageEntity = await _messageRepository.GetById(id); 
        if (currentUser?.Id != messageEntity?.UserId)
        {
            return false;
        }
        return await _messageRepository.Delete(id);
    }
}