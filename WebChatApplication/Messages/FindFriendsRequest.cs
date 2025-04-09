using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Enums;

namespace WebChatApplication.Messages;

public class FindFriendsRequest
{
    [BindProperty(Name = "username")] public string? Username { get; set; }
    [BindProperty(Name = "page")] public int PageNumber { get; set; } = 1;
    [BindProperty(Name = "pageSize")] public int PageSize { get; set; } = 10;
    [BindProperty(Name = "sortOrder")] public FriendSortState SortOrder { get; set; } = FriendSortState.UsernameAsc;
}