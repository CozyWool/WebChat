using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class BlacklistedUserConfiguration : IEntityTypeConfiguration<BlacklistedUserEntity>
{
    public void Configure(EntityTypeBuilder<BlacklistedUserEntity> builder)
    {
        builder.HasKey(e => new {e.UserId, e.BlacklistedUserId}).HasName("BlacklistedUsers_pkey");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.BlacklistedUserId).HasColumnName("friend_user_id");

        builder.HasOne(d => d.BlacklistedUser).WithMany()
            .HasForeignKey(d => d.BlacklistedUserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("friend_user_id_fk");

        builder.HasOne(d => d.User)
            .WithMany(p => p.BlacklistedUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_id_fk");
    }
}