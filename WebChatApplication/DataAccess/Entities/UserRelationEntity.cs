using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Entities;

public class UserRelationEntity
{
    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public virtual UserEntity ToUser { get; set; } = null!;

    public virtual UserEntity FromUser { get; set; } = null!;
    public UserRelationTypes RelationType { get; set; }
}