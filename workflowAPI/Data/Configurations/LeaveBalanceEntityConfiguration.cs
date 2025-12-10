using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class LeaveBalanceEntityConfiguration : IEntityTypeConfiguration<LeaveBalanceEntity>
    {
        public void Configure(EntityTypeBuilder<LeaveBalanceEntity> builder)
        {
            builder.ToTable("LeaveBalances");

            builder.HasKey(lb => lb.Id);

            builder.Property(lb => lb.UserId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(lb => lb.TotalDays)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(lb => lb.UsedDays)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            builder.Property(lb => lb.PendingDays)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            builder.Property(lb => lb.CarryForwardDays)
                .HasColumnType("decimal(5,2)");

            // Computed column for AvailableDays
            builder.Property(lb => lb.AvailableDays)
                .HasComputedColumnSql("[TotalDays] - [UsedDays] - [PendingDays]", stored: true);

            builder.Property(lb => lb.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on UserId, LeaveTypeId, Year
            builder.HasIndex(lb => new { lb.UserId, lb.LeaveTypeId, lb.Year })
                .IsUnique()
                .HasDatabaseName("UQ_LeaveBalances_UserTypeYear");

            // Composite index for common queries
            builder.HasIndex(lb => lb.UserId)
                .HasDatabaseName("IX_LeaveBalances_UserId");

            builder.HasIndex(lb => lb.Year)
                .HasDatabaseName("IX_LeaveBalances_Year");

            // Check constraint for AvailableDays
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_LeaveBalances_AvailableDays",
                "[TotalDays] - [UsedDays] - [PendingDays] >= 0"));

            // Relationships
            builder.HasOne(lb => lb.User)
                .WithMany(u => u.LeaveBalances)
                .HasForeignKey(lb => lb.UserId);

            builder.HasOne(lb => lb.LeaveType)
                .WithMany(lt => lt.LeaveBalances)
                .HasForeignKey(lb => lb.LeaveTypeId);
        }
    }
}
