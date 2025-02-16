namespace WebChatApplication.DataAccess.Entities;

public class BlacklistedUserEntity
{
    public Guid UserId { get; set; }

    public Guid BlacklistedUserId { get; set; }

    public virtual UserEntity BlacklistedUser { get; set; } = null!;

    public virtual UserEntity User { get; set; } = null!;
}