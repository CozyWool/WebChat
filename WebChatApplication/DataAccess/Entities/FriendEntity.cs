namespace WebChatApplication.DataAccess.Entities;

public class FriendEntity
{
    public Guid UserId { get; set; }
    public Guid FriendUserId { get; set; }
    public bool IsAccepted { get; set; }
    public virtual UserEntity FriendUser { get; set; } = null!;

    public virtual UserEntity User { get; set; } = null!;
}