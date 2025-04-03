using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User;

public class LoginModel
{
    [Required(ErrorMessage = "Укажите эл.почту/имя пользователя")]
    [Display(Name = "Эл.почта/Имя пользователя")]
    public string UsernameOrEmail { get; set; }

    [Required(ErrorMessage = "Укажите пароль")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Запомнить")] public bool RememberMe { get; set; }
}