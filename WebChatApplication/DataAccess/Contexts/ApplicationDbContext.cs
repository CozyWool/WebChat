using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Configuration;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public virtual DbSet<UserEntity> Users { get; set; }
    public virtual DbSet<FriendEntity> Friends { get; set; }
    public virtual DbSet<BlacklistedUserEntity> BlacklistedUsers { get; set; }
    public virtual DbSet<RoleEntity> Roles { get; set; }
    public virtual DbSet<ChatEntity> Chats { get; set; }
    public virtual DbSet<MessageEntity> Messages { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    }
}