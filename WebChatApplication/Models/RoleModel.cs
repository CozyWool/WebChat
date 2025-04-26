using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models;

public class RoleModel
{
    [Display(Name = "Id")] public int Id { get; set; }
    [Display(Name = "Название")] public string Name { get; set; }
}