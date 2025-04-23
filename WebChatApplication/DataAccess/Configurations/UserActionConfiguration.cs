using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configurations;

public class UserActionConfiguration : IEntityTypeConfiguration<UserActionEntity>
{
    public void Configure(EntityTypeBuilder<UserActionEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_actions_pkey");

        builder.ToTable("user_actions");

        builder.Property(e => e.Id)
               .HasColumnName("id");
        builder.Property(e => e.UserId)
               .HasColumnName("user_id");
        builder.Property(e => e.Action)
               .HasColumnName("action");

        builder.HasOne(e => e.User)
               .WithMany(e => e.Actions)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .IsRequired();
    }
}