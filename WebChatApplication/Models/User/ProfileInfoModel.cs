using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User;

public class ProfileInfoModel
{
    [Display(Name = "Имя пользователя")] public string Username { get; set; }
    [Display(Name = "Роль")] public string RoleName { get; set; }

    [Display(Name = "Дата регистрации")]
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Последняя активность")]
    [DataType(DataType.Date)]
    public DateTime? LastActivityAt { get; set; }

    public bool IsOnline { get; set; }
    public string ProfilePictureUrl { get; set; }
}