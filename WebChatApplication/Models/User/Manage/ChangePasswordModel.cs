using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.Manage;

public class ChangePasswordModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
}