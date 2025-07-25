namespace WebChatApplication.DataAccess.Entities;

public class AttachmentEntity
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string Path { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public virtual MessageEntity Message { get; set; } = null!;
}