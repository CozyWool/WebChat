using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User;

public class RegisterModel
{
    [Required(ErrorMessage = "Укажите эл. почту")]
    [Display(Name = "Эл. почта")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Укажите имя пользователя")]
    // TODO: наложить ограничения 
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Укажите пароль")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
}