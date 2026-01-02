namespace workflowAPI.Models.Entities
{
    public class LeaveRequestEntity
    {
        public int Id { get; set; }
        public string LeaveRequestId { get; set; } = string.Empty; // Maps to Workflow ProcessId
        public string UserId { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SelectedApproverId { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public string? CurrentWorkflowState { get; set; }
        public string? WorkflowProcessId { get; set; }
        public string? WorkflowSchemeCode { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual UserEntity User { get; set; } = null!;
        public virtual LeaveTypeEntity LeaveType { get; set; } = null!;
        public virtual UserEntity? SelectedApprover { get; set; }
        public virtual ICollection<LeaveApprovalEntity> Approvals { get; set; } = new List<LeaveApprovalEntity>();
    }

    public enum LeaveRequestStatus
    {

        LeaveRequestCreated,
        ManagerSigning,
        HRSigning,
        Rejected,
        Approved,
        Cancelled

    }
}
