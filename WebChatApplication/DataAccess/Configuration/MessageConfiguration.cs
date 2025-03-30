using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("messages_pkey");

        builder.ToTable("messages");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        builder.Property(e => e.ChatId)
            .HasColumnName("chat_id");
        builder.Property(e => e.ParentMessageId)
            .HasColumnName("parent_message_id");
        builder.Property(e => e.SentAt)
            .HasColumnName("sent_at");
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        builder.Property(e => e.Content)
            .HasColumnName("content");

        builder.HasOne(e => e.User)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .IsRequired();

        builder.HasOne(e => e.ParentMessage)
            .WithOne()
            .HasForeignKey<MessageEntity>(e => e.ParentMessageId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}