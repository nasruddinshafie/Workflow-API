using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Configurations
{
    public class LeaveApprovalEntityConfiguration : IEntityTypeConfiguration<LeaveApprovalEntity>
    {
        public void Configure(EntityTypeBuilder<LeaveApprovalEntity> builder)
        {
            builder.ToTable("LeaveApprovals");

            builder.HasKey(la => la.Id);

            builder.Property(la => la.ApproverId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(la => la.ApproverRole)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(la => la.Action)
                .HasMaxLength(20)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(la => la.Comments)
                .HasMaxLength(1000);

            builder.Property(la => la.ActionDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(la => la.LeaveRequestId)
                .HasDatabaseName("IX_LeaveApprovals_LeaveRequestId");

            builder.HasIndex(la => la.ApproverId)
                .HasDatabaseName("IX_LeaveApprovals_ApproverId");

            builder.HasIndex(la => la.ActionDate)
                .HasDatabaseName("IX_LeaveApprovals_ActionDate");

            // Relationships
            builder.HasOne(la => la.LeaveRequest)
                .WithMany(lr => lr.Approvals)
                .HasForeignKey(la => la.LeaveRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(la => la.Approver)
                .WithMany()
                .HasForeignKey(la => la.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
