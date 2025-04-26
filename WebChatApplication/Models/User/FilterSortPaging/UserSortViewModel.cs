using System.ComponentModel.DataAnnotations;
using WebChatApplication.Enums;

namespace WebChatApplication.Models.User.FilterSortPaging;

public class UserSortViewModel(UserSortState sortOrder)
{
    public UserSortState UsernameSort { get; } =
        sortOrder == UserSortState.UsernameAsc
            ? UserSortState.UsernameDesc
            : UserSortState.UsernameAsc;

    public UserSortState LastActivitySort { get; } =
        sortOrder == UserSortState.LastActivityAsc
            ? UserSortState.LastActivityDesc
            : UserSortState.LastActivityAsc;

    [Display(Name = "Сортировать по")] public UserSortState Current { get; } = sortOrder;
}