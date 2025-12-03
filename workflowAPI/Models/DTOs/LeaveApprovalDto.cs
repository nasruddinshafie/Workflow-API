namespace workflowAPI.Models.DTOs
{
    public class LeaveApprovalDto
    {
        public string LeaveId { get; set; } = string.Empty;
        public string ApproverId { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public bool Approved { get; set; }
    }
}
