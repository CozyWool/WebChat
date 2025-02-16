namespace WebChatApplication.DataAccess.Entities;

public class ChatEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    public virtual ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
}