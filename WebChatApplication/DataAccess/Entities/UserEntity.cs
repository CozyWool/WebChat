using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public int RoleId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordRecoveryToken { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string ProfilePictureFileName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public UserStatuses Status { get; set; }
    public DateTime? LastActivity { get; set; }
    public virtual RoleEntity Role { get; set; } = null!;
    public virtual ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public virtual ICollection<UserRelationEntity> RelatedUsers { get; set; } = new List<UserRelationEntity>();
    public virtual ICollection<ChatEntity> Chats { get; set; } = new List<ChatEntity>();
    public virtual ICollection<UserActionEntity> Actions { get; set; } = new List<UserActionEntity>();
}