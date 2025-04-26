using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;

namespace WebChatApplication.Models.User.FilterSortPaging;

public class UserFilterViewModel
{
    public UserFilterViewModel(string? selectedUsername, UserRelationTypes? selectedRelationType,
                               int? selectedRole, List<RoleModel> roles, bool isNeedToFilterRelationTypes = true,
                               bool isNeedToFilterRoles = true)
    {
        SelectedUsername = selectedUsername;
        SelectedRelationType = selectedRelationType;
        SelectedRole = selectedRole;
        IsNeedToFilterRelationTypes = isNeedToFilterRelationTypes;
        IsNeedToFilterRoles = isNeedToFilterRoles;
        Roles = new SelectList(roles, "Id", "Name");
        foreach (var role in Roles)
        {
            role.Text = role.Text switch
                        {
                            "User"       => "Пользователь",
                            "Admin"      => "Администратор",
                            "SuperAdmin" => "Супер-Администратор"
                        };
        }
    }

    [Display(Name = "Имя пользователя")] public string? SelectedUsername { get; }
    [Display(Name = "Роль")] public int? SelectedRole { get; }
    [Display(Name = "Взаимоотношения")] public UserRelationTypes? SelectedRelationType { get; }
    public IEnumerable<SelectListItem> RelationTypes { get; set; }
    public bool IsNeedToFilterRelationTypes { get; }
    public bool IsNeedToFilterRoles { get; }
    public IEnumerable<SelectListItem> Roles { get; set; }
}