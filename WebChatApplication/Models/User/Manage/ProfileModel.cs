using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.Manage;

public class ProfileModel
{
    public StatusMessageModel StatusMessage { get; set; } = new();

    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }
    [Display(Name = "Роль")]
    public string RoleName { get; set; }
    public string OldUsername { get; set; }
    [Display(Name = "Дата создания")]
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
}