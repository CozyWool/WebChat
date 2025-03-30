using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class UserRelationConfiguration : IEntityTypeConfiguration<UserRelationEntity>
{
    public void Configure(EntityTypeBuilder<UserRelationEntity> builder)
    {
        builder.HasKey(e => new {UserId = e.FromUserId, BlacklistedUserId = e.ToUserId}).HasName("user_relation_pkey");

        builder.ToTable("user_relations");

        builder.Property(e => e.FromUserId).HasColumnName("from_user_id");
        builder.Property(e => e.ToUserId).HasColumnName("to_user_id");
        builder.Property(e => e.RelationType).HasColumnName("relation_type");

        builder.HasOne(d => d.RelatedUser).WithMany()
            .HasForeignKey(d => d.ToUserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("to_user_id_fk");

        builder.HasOne(d => d.User)
            .WithMany(p => p.RelatedUsers)
            .HasForeignKey(d => d.FromUserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("from_user_id_fk");
    }
}