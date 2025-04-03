using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.Manage;

public class DeleteAccountModel
{
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
}