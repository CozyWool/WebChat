using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class AttachmentConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("attachments_pkey");

        builder.ToTable("attachments");

        builder.Property(e => e.Id)
               .HasColumnName("id");
        builder.Property(e => e.MessageId)
               .HasColumnName("message_id");
        builder.Property(e => e.Path)
               .HasColumnName("path")
               .HasMaxLength(500);

        builder.HasOne(e => e.Message)
               .WithMany(e => e.Attachments)
               .HasForeignKey(e => e.MessageId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .IsRequired();
    }
}