using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;
using WebChatApplication.Enums;
using WebChatApplication.Helpers;

namespace WebChatApplication.DataAccess.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("users_pkey");

        builder.ToTable("users");

        builder.Property(e => e.Id)
               .ValueGeneratedNever()
               .HasColumnName("id");
        builder.Property(e => e.RoleId)
               .ValueGeneratedNever()
               .HasColumnName("role_id");
        builder.Property(e => e.Username)
               .HasMaxLength(75)
               .HasColumnName("username");
        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at");
        builder.Property(e => e.Email)
               .HasMaxLength(100)
               .HasColumnName("email");
        builder.Property(e => e.EmailConfirmationToken)
               .HasMaxLength(100)
               .HasColumnName("email_confirmation_token");
        builder.Property(e => e.PasswordRecoveryToken)
               .HasMaxLength(100)
               .HasColumnName("password_recovery_token");
        builder.Property(e => e.ProfilePictureFileName)
               .HasMaxLength(300)
               .ValueGeneratedNever()
               .HasColumnName("profile_picture_file_name");
        builder.Property(e => e.Status)
               .HasDefaultValue(UserStatuses.EmailNotConfirmed)
               .HasColumnName("status");
        builder.Property(e => e.LastActivityAt)
               .HasColumnName("last_activity_at")
               .IsRequired(false);

        builder.Property(e => e.PasswordHash)
               .HasColumnName("password_hash")
               .HasMaxLength(100);

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
               .UsingEntity(joinEntity =>
                            {
                                joinEntity.Property("UsersId").HasColumnName("user_id");
                                joinEntity.Property("ChatsId").HasColumnName("chat_id");
                                joinEntity.ToTable("users_chats");
                            });

        builder.HasMany(e => e.Actions)
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId);

        builder.HasIndex(u => u.Username)
               .IsUnique();
        builder.HasIndex(u => u.Email)
               .IsUnique();

        var id = new Guid("df6b1476-c214-4103-90d0-6fae25e74bca");
        builder.HasData(
                        new UserEntity
                        {
                            Id = id,
                            RoleId = 3,
                            Username = "admin",
                            Email = "admin@gmail.com",
                            // TODO: Задавать пароль через config
                            PasswordHash = SecurityHelper.GenerateSaltedHash("admin",
                                                                             id.ToString()),
                            CreatedAt = DateTime.MinValue,
                            Status = UserStatuses.Active,
                            LastActivityAt = DateTime.MinValue
                        });
    }
}