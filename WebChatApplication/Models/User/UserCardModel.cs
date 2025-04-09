using System.ComponentModel.DataAnnotations;
using WebChatApplication.Enums;

namespace WebChatApplication.Models.User;

public class UserCardModel
{
    [Display(Name = "Имя пользователя")] public string Username { get; set; }

    [Display(Name = "Последняя активность")]
    [DataType(DataType.Date)]
    public DateTime? LastActivityAt { get; set; }

    public bool IsOnline { get; set; }
    public string ProfilePictureUrl { get; set; }
    public UserRelationTypes RelationType { get; set; }
}