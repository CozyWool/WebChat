using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Enums;

public enum FriendSortState
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