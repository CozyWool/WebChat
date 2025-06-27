using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class WebChatViewModel
{
    public ChatModel? CurrentChat { get; set; }
    public List<ChatModel> Chats { get; set; }
    public List<UserCardModel> Friends { get; set; }
}