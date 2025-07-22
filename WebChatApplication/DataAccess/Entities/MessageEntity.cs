namespace WebChatApplication.DataAccess.Entities;

public class MessageEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChatId { get; set; }
    public Guid? ParentMessageId { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Content { get; set; } = null!;
    public bool IsServiceMessage { get; set; }
    public virtual MessageEntity? ParentMessage { get; set; }
    public virtual UserEntity User { get; set; } = null!;
    public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
}