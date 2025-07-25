using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Configurations;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public virtual DbSet<UserEntity> Users { get; set; }
    public virtual DbSet<UserRelationEntity> UserRelations { get; set; }
    public virtual DbSet<RoleEntity> Roles { get; set; }
    public virtual DbSet<ChatEntity> Chats { get; set; }
    public virtual DbSet<MessageEntity?> Messages { get; set; }
    public virtual DbSet<AttachmentEntity> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    }
}