using System.ComponentModel.DataAnnotations;
using WebChatApplication.Attributes;

namespace WebChatApplication.Models.User.Manage;

public class ChangeProfileInfoModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [Display(Name = "Имя пользователя")] public string Username { get; set; }
    [Display(Name = "Роль")] public string RoleName { get; set; }
    public string OldUsername { get; set; }

    [Display(Name = "Дата регистрации")]
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Фото профиля")]
    [DataType(DataType.Upload)]
    [HttpPostedFileExtensions(Extensions = "jpg,jpeg,png",
                                 ErrorMessage = "Допустимые расширения файла: .jpg, .jpeg, .png")]
    public IFormFile? ProfilePicture { get; set; }
}