using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class GroupChatModel : ChatModel
{
    public string ChatPictureUrl { get; set; }
    public bool IsCurrentUserChatOwner { get; set; }
}