namespace WebChatApplication.DataAccess.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public int RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
    public DateTime LastActivity { get; set; }
    public virtual RoleEntity Role { get; set; } = null!;
    public virtual ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public virtual ICollection<FriendEntity> Friends { get; set; } = new List<FriendEntity>();

    public virtual ICollection<BlacklistedUserEntity> BlacklistedUsers { get; set; } = new List<BlacklistedUserEntity>();


    public virtual ICollection<ChatEntity> Chats { get; set; } = new List<ChatEntity>();
}