using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.PasswordRecovery;

public class SendPasswordRecoveryEmailModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [EmailAddress]
    [Display(Name = "Электронная почта")]
    public string? Email { get; set; }

    public string? PasswordRecoveryToken { get; set; }
    public string? Username { get; set; }
    public string? PasswordRecoveryUrl { get; set; }
}