using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.Models.User;

public class UserAndRelationTypeRecord(UserEntity toUser, UserRelationTypes relationType)
{
    public UserEntity ToUser { get; set; } = toUser;
    public UserRelationTypes RelationType { get; set; } = relationType;
}