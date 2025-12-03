namespace workflowAPI.Models.DTOs
{
    public class LeaveStatusDto
    {
        public string LeaveId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string SchemeVersion { get; set; } = string.Empty;
        public List<string> AvailableActions { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
