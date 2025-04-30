using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;
using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class ChatModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ChatTypes ChatType { get; set; }
    public UserCardModel CurrentUser { get; set; }
    public List<MessageModel> Messages { get; set; }
}