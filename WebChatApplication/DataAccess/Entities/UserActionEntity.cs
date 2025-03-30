namespace WebChatApplication.DataAccess.Entities;

public class UserActionEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = null!;
    public virtual UserEntity User { get; set; } = null!;
}