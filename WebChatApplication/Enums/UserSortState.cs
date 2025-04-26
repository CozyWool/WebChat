using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Enums;

public enum UserSortState
{
    [Display(Name = "Имя пользователя ↑")]
    UsernameAsc,

    [Display(Name = "Имя пользователя ↓")]
    UsernameDesc,

    [Display(Name = "Последняя активность ↑")]
    LastActivityAsc,

    [Display(Name = "Последняя активность ↓")]
    LastActivityDesc,
}