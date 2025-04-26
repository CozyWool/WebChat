using WebChatApplication.Models;

namespace WebChatApplication.Services;

public interface IRoleService
{
    Task<List<RoleModel>> GetAll();
    Task<RoleModel?> GetByName(string name);
}