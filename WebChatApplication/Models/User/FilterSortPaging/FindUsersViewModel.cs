using WebChatApplication.Messages;

namespace WebChatApplication.Models.User.FilterSortPaging;

public class FindUsersViewModel(
    List<UserCardModel> users,
    int count,
    FindUsersRequest request,
    List<RoleModel> roles,
    bool isNeedToFilterRelationTypes = true,
    bool isNeedToFilterRoles = true)
{
    public FindUsersViewModel() : this([], 0, new FindUsersRequest(), [])
    {
    }

    public List<UserCardModel> Users { get; set; } = users;

    public UserPageViewModel PageViewModel { get; set; } =
        new(count,
            request.PageNumber,
            request.PageSize);

    public UserFilterViewModel FilterViewModel { get; set; } =
        new(request.Username,
            request.RelationType,
            request.RoleId,
            roles,
            isNeedToFilterRelationTypes,
            isNeedToFilterRoles);

    public UserSortViewModel SortViewModel { get; set; } =
        new(request.SortOrder);
}