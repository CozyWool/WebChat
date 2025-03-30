using WebChatApplication.Enums;

namespace WebChatApplication.DataAccess.Entities;

public class UserRelationEntity
{
    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public virtual UserEntity RelatedUser { get; set; } = null!;

    public virtual UserEntity User { get; set; } = null!;
    public UserRelationTypes RelationType { get; set; }
}