using System.ComponentModel.DataAnnotations;
using WebChatApplication.Enums;

namespace WebChatApplication.Models.User.FilterSortPagingFriends;

public class FriendSortViewModel(FriendSortState sortOrder)
{
    public FriendSortState UsernameSort { get; } =
        sortOrder == FriendSortState.UsernameAsc
            ? FriendSortState.UsernameDesc
            : FriendSortState.UsernameAsc;

    public FriendSortState LastActivitySort { get; } =
        sortOrder == FriendSortState.LastActivityAsc
            ? FriendSortState.LastActivityDesc
            : FriendSortState.LastActivityAsc;

    [Display(Name = "Сортировать по")] public FriendSortState Current { get; } = sortOrder;
}