namespace WebChatApplication.Models;

public class AttachmentModel
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = null!;
    public string PathUrl { get; set; } = null!;
}