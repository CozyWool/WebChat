using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.FilterSortPagingFriends;

public class FriendFilterViewModel
{
    public FriendFilterViewModel(string? selectedUsername)
    {
        SelectedUsername = selectedUsername;
    }

    [Display(Name = "Имя пользователя")] public string? SelectedUsername { get; }
}