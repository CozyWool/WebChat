using Microsoft.EntityFrameworkCore;
using WebChatApplication.Configurations;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Services;

namespace WebChatApplication.DataAccess.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessageRepository _messageRepository;
    private readonly IS3Service _s3Service;
    private readonly string _bucketId;

    public AttachmentRepository(ApplicationDbContext dbContext,
                                IMessageRepository messageRepository,
                                IS3Service s3Service,
                                IConfiguration configuration)
    {
        _dbContext = dbContext;
        _messageRepository = messageRepository;
        _s3Service = s3Service;
        _bucketId = configuration.GetSection("MinioConfiguration").Get<MinioConfiguration>().BucketId;
    }

    public async Task Create(AttachmentEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        await _dbContext.Attachments.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(AttachmentEntity entity)
    {
        var oldEntity = await GetById(entity.Id);
        if (oldEntity is null)
        {
            return;
        }

        oldEntity.Path = entity.Path;
        _dbContext.Attachments.Update(oldEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await GetById(id);
        return await Delete(entity);
    }

    public async Task<bool> Delete(AttachmentEntity? entity)
    {
        if (entity is null)
        {
            return false;
        }

        if (!await _s3Service.DeleteFile(_bucketId, entity.Path))
        {
            return false;
        }

        _dbContext.Attachments.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllByMessageId(Guid messageId)
    {
        var attachments = await GetByMessageId(messageId);
        if (attachments.Count == 0)
        {
            return true;
        }

        foreach (var attachment in attachments)
        {
            if (!await Delete(attachment))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<AttachmentEntity?> GetById(Guid id)
    {
        return await _dbContext
                     .Attachments
                     .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AttachmentEntity>> GetByMessageId(Guid messageId)
    {
        var message = await _messageRepository.GetById(messageId);
        if (message is null)
        {
            return [];
        }

        return await _dbContext
                     .Attachments
                     .Where(x => x.MessageId == messageId)
                     .ToListAsync();
    }
}