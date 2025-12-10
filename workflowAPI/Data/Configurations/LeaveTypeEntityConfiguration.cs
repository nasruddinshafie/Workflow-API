using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class LeaveTypeEntityConfiguration : IEntityTypeConfiguration<LeaveTypeEntity>
    {
        public void Configure(EntityTypeBuilder<LeaveTypeEntity> builder)
        {
            builder.ToTable("LeaveTypes");

            builder.HasKey(lt => lt.Id);

            builder.Property(lt => lt.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(lt => lt.Code)
                .IsUnique()
                .HasDatabaseName("IX_LeaveTypes_Code");

            builder.Property(lt => lt.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(lt => lt.Description)
                .HasMaxLength(500);

            builder.Property(lt => lt.DefaultDaysPerYear)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(lt => lt.Color)
                .HasMaxLength(20);

            builder.HasIndex(lt => lt.IsActive)
                .HasDatabaseName("IX_LeaveTypes_IsActive");

            builder.Property(lt => lt.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
