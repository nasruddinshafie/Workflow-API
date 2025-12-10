using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");

            builder.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.Department)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(u => u.Department)
                .HasDatabaseName("IX_Users_Department");

            builder.Property(u => u.ManagerId)
                .HasMaxLength(50);

            builder.HasIndex(u => u.ManagerId)
                .HasDatabaseName("IX_Users_ManagerId");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            builder.Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Self-referencing relationship
            builder.HasOne(u => u.Manager)
                .WithMany(u => u.DirectReports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many with Roles
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);
        }
    }
}
