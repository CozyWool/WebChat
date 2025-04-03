using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.PasswordRecovery;

public class PasswordRecoveryModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }

    public string? Email { get; set; }
    public string? PasswordRecoveryToken { get; set; }
}