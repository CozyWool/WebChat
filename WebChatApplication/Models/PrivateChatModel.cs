using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class PrivateChatModel : ChatModel
{ 
    public UserCardModel PrivateUser { get; set; }
}