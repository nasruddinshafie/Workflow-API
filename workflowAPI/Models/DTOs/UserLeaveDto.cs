namespace workflowAPI.Models.DTOs
{
    public class UserLeaveDto
    {
        public int Id { get; set; }
        public string LeaveRequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string? LeaveTypeColor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SelectedApproverId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CurrentWorkflowState { get; set; }
        public string? WorkflowProcessId { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<LeaveApprovalInfoDto> Approvals { get; set; } = new List<LeaveApprovalInfoDto>();
    }

    public class LeaveApprovalInfoDto
    {
        public int Id { get; set; }
        public string ApproverId { get; set; } = string.Empty;
        public string ApproverFullName { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
