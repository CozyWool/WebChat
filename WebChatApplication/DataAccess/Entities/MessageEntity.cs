namespace WebChatApplication.DataAccess.Entities;

public class MessageEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChatId { get; set; }
    public DateTime SentDate { get; set; }
    public string Content { get; set; } = null!;
    public virtual UserEntity User { get; set; } = null!;
}