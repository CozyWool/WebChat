using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Entities;

public class ChatEntity
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public ChatTypes ChatType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ChatPictureFileName { get; set; } = null!;
    public virtual UserEntity Owner { get; set; } = null!;
    public virtual ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    public virtual ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
}