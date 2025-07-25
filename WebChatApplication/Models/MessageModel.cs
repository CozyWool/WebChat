using WebChatApplication.Models.User;

namespace WebChatApplication.Models;

public class MessageModel
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserCardModel Author { get; set; }
    public MessageModel? ParentMessage { get; set; }
    public List<AttachmentModel> Attachments { get; set; }
    public bool IsCurrentUserSentMessage { get; set; }
    public bool IsServiceMessage { get; set; }
}