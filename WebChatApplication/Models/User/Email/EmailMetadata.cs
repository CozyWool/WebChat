namespace WebChatApplication.Models.User.Email;

public class EmailMetadata(string toAddress, string subject, string? body = "", bool isHtml = false)
{
    public string ToAddress { get; set; } = toAddress;
    public string Subject { get; set; } = subject;
    public string? Body { get; set; } = body;
    public bool IsHtml { get; set; } = isHtml;
}