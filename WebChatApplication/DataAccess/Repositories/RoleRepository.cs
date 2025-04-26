using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Repositories;

public class RoleRepository : IRoleRepository
{
    private ApplicationDbContext _dbContext;

    public RoleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoleEntity?>> GetAll() =>
        await _dbContext.Roles.ToListAsync();

    public async Task<RoleEntity?> GetByName(string name) =>
        await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == name);
}