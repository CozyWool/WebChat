namespace WebChatApplication.Models.User.Email;

public class EmailUserModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string EmailConfirmationToken { get; set; }
    public string EmailConfirmationUrl { get; set; }
}