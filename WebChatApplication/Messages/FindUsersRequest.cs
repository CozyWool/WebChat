using Microsoft.AspNetCore.Mvc;
using WebChatApplication.Enums;

namespace WebChatApplication.Messages;

public class FindUsersRequest
{
    private int _pageSize = 10;
    [BindProperty(Name = "username")] public string? Username { get; set; }
    [BindProperty(Name = "role")] public int? Role { get; set; }
    [BindProperty(Name = "relationType")] public UserRelationTypes? RelationType { get; set; }
    [BindProperty(Name = "page")] public int PageNumber { get; set; } = 1;

    [BindProperty(Name = "pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 ? value : 1;
    }

    [BindProperty(Name = "sortOrder")] public UserSortState SortOrder { get; set; } = UserSortState.UsernameAsc;
}