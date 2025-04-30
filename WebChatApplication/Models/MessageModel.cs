namespace WebChatApplication.Models;

public class MessageModel
{
    
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsCurrentUserSentMessage { get; set; }
}