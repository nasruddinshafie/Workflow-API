using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class LeaveRequestEntityConfiguration : IEntityTypeConfiguration<LeaveRequestEntity>
    {
        public void Configure(EntityTypeBuilder<LeaveRequestEntity> builder)
        {
            builder.ToTable("LeaveRequests");

            builder.HasKey(lr => lr.Id);

            builder.Property(lr => lr.LeaveRequestId)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(lr => lr.LeaveRequestId)
                .IsUnique()
                .HasDatabaseName("IX_LeaveRequests_LeaveRequestId");

            builder.Property(lr => lr.UserId)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(lr => lr.UserId)
                .HasDatabaseName("IX_LeaveRequests_UserId");

            builder.Property(lr => lr.StartDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(lr => lr.EndDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(lr => lr.TotalDays)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(lr => lr.Reason)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(lr => lr.SelectedApproverId)
                .HasMaxLength(50);

            builder.Property(lr => lr.Status)
                .HasMaxLength(50)
                .HasConversion<string>()
                .IsRequired();

            builder.HasIndex(lr => lr.Status)
                .HasDatabaseName("IX_LeaveRequests_Status");

            builder.Property(lr => lr.CurrentWorkflowState)
                .HasMaxLength(100);

            builder.Property(lr => lr.WorkflowProcessId)
                .HasMaxLength(100);

            builder.HasIndex(lr => lr.WorkflowProcessId)
                .HasDatabaseName("IX_LeaveRequests_WorkflowProcessId");

            builder.Property(lr => lr.WorkflowSchemeCode)
                .HasMaxLength(100);

            builder.Property(lr => lr.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Index for date range queries
            builder.HasIndex(lr => new { lr.StartDate, lr.EndDate })
                .HasDatabaseName("IX_LeaveRequests_Dates");

            // Relationships
            builder.HasOne(lr => lr.User)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(lr => lr.LeaveType)
                .WithMany(lt => lt.LeaveRequests)
                .HasForeignKey(lr => lr.LeaveTypeId);

            builder.HasOne(lr => lr.SelectedApprover)
                .WithMany()
                .HasForeignKey(lr => lr.SelectedApproverId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
