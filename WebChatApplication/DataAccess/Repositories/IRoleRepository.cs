using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public interface IRoleRepository
{
    Task<List<RoleEntity?>> GetAll();
    Task<RoleEntity?> GetByName(string name);
}