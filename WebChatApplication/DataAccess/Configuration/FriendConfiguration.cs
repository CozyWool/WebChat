using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class FriendConfiguration : IEntityTypeConfiguration<FriendEntity>
{
    public void Configure(EntityTypeBuilder<FriendEntity> builder)
    {
        builder.HasKey(e => new {e.UserId, e.FriendUserId}).HasName("Friends_pkey");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.FriendUserId).HasColumnName("friend_user_id");
        builder.Property(e => e.IsAccepted)
            .HasDefaultValue(false)
            .HasColumnName("is_accepted");

        builder.HasOne(d => d.FriendUser).WithMany()
            .HasForeignKey(d => d.FriendUserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("friend_user_id_fk");

        builder.HasOne(d => d.User)
            .WithMany(p => p.Friends)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_id_fk");
    }
}