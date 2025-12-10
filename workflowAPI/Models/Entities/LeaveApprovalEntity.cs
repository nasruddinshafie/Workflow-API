namespace workflowAPI.Models.Entities
{
    public class LeaveApprovalEntity
    {
        public int Id { get; set; }
        public int LeaveRequestId { get; set; }
        public string ApproverId { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty;
        public ApprovalAction Action { get; set; }
        public string? Comments { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual LeaveRequestEntity LeaveRequest { get; set; } = null!;
        public virtual UserEntity Approver { get; set; } = null!;
    }

    public enum ApprovalAction
    {
        Approved,
        Rejected
    }
}
