using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class ChatConfiguration : IEntityTypeConfiguration<ChatEntity>
{
    public void Configure(EntityTypeBuilder<ChatEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("Chats_pkey");
        
        builder.ToTable("Chats");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .HasColumnName("name");
        builder.Property(e => e.CreatedDate)
            .HasColumnName("created_date");

        builder.HasMany(e => e.Users)
            .WithMany(e => e.Chats)
            .UsingEntity(joinEntity => joinEntity.ToTable("UsersChats"));
        builder.HasMany(e => e.Messages)
            .WithOne()
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("chat_id_fk");
    }
}