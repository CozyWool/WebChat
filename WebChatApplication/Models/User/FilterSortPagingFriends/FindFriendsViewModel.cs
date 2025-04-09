namespace WebChatApplication.Models.User.FilterSortPagingFriends;

public class FindFriendsViewModel
{
    public List<UserCardModel> Users { get; set; }
    public FriendPageViewModel PageViewModel { get; set; }
    public FriendFilterViewModel FilterViewModel { get; set; }
    public FriendSortViewModel SortViewModel { get; set; }
}