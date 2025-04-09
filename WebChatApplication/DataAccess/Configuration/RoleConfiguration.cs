using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebChatApplication.DataAccess.Entities;

namespace WebChatApplication.DataAccess.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.HasKey(e => e.Id).HasName("roles_pkey");

        builder.ToTable("roles");

        builder.Property(e => e.Id)
               .HasColumnName("id");
        builder.Property(e => e.Name)
               .HasMaxLength(100)
               .HasColumnName("name");

        builder.HasData(
                        new RoleEntity
                        {
                            Id = 1,
                            Name = "User"
                        }, new RoleEntity
                           {
                               Id = 2,
                               Name = "Admin"
                           }, new RoleEntity
                              {
                                  Id = 3,
                                  Name = "SuperAdmin"
                              });
    }
}