using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class MessageModel
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public UserCardModel Author { get; set; }
    public bool IsCurrentUserSentMessage { get; set; }
}