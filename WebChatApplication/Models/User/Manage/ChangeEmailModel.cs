using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.Manage;

public class ChangeEmailModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [EmailAddress]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; }

    [EmailAddress]
    [Display(Name = "Новая электронная почта")]
    public string NewEmail { get; set; }
}