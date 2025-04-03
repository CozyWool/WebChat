using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.Email;

public class EmailConfirmationModel
{
    [EmailAddress]
    [Display(Name = "Электронная почта")]
    public string? Email { get; set; }
    public string? Token { get; set; }
    public StatusMessageModel StatusMessage { get; set; } = new();
    public bool NeedToSend { get; set; }
}