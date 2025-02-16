using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("Users_pkey");

        builder.ToTable("Users");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(e => e.RoleId)
            .ValueGeneratedNever()
            .HasColumnName("role_id");
        builder.Property(e => e.Username)
            .HasMaxLength(75)
            .HasColumnName("username");
        builder.Property(e => e.CreatedDate)
            .HasColumnName("created_date");
        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .HasColumnName("email");
        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false)
            .HasColumnName("is_deleted");
        builder.Property(e => e.IsBanned)
            .HasDefaultValue(false)
            .HasColumnName("is_banned");
        builder.Property(e => e.LastActivity)
            .HasColumnName("last_activity");

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash");

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("role_id_fk");

        builder.HasMany(e => e.Messages)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasMany(e => e.Chats)
            .WithMany(e => e.Users)
            .UsingEntity(joinEntity => joinEntity.ToTable("UsersChats"));
    }
}