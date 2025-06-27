using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatEntity>
{
    public void Configure(EntityTypeBuilder<ChatEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("chats_pkey");

        builder.ToTable("chats");

        builder.Property(e => e.Id)
               .HasColumnName("id");
        builder.Property(e => e.OwnerId)
               .HasColumnName("owner_id");
        builder.Property(e => e.ChatType)
               .HasColumnName("chat_type");
        builder.Property(e => e.Name)
               .HasMaxLength(100)
               .HasColumnName("name");
        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at");
        builder.Property(e => e.ChatPictureFileName)
               .HasMaxLength(300)
               .ValueGeneratedNever()
               .HasColumnName("chat_picture_file_name");

        builder.HasOne(e => e.Owner)
               .WithMany()
               .HasForeignKey(e => e.OwnerId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("owner_id_fk");

        builder.HasMany(e => e.Users)
               .WithMany(e => e.Chats)
               .UsingEntity(joinEntity =>
                            {
                                joinEntity.Property("UsersId").HasColumnName("user_id");
                                joinEntity.Property("ChatsId").HasColumnName("chat_id");
                                joinEntity.ToTable("users_chats");
                            });

        builder.HasMany(e => e.Messages)
               .WithOne()
               .HasForeignKey(e => e.ChatId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("chat_id_fk");
    }
}