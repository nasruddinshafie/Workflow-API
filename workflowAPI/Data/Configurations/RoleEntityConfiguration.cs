using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RoleName)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(r => r.RoleName)
                .IsUnique()
                .HasDatabaseName("IX_Roles_RoleName");

            builder.Property(r => r.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.Property(r => r.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Many-to-many with Users
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
