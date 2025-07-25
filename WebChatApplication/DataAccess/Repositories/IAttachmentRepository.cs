using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IAttachmentRepository
{
    Task Create(AttachmentEntity? entity);
    Task Update(AttachmentEntity entity);
    Task<bool> Delete(Guid id);
    Task<bool> Delete(AttachmentEntity? entity);
    Task<bool> DeleteAllByMessageId(Guid messageId);
    Task<AttachmentEntity?> GetById(Guid id);
    Task<List<AttachmentEntity>> GetByMessageId(Guid messageId);
}