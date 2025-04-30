using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.Models;

public class GroupChatModel : ChatModel
{
    public UserEntity Owner { get; set; }
    public List<UserEntity> Users { get; set; }
    public string ChatPictureUrl { get; set; }
}