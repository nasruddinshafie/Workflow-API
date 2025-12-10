namespace workflowAPI.Models.DTOs
{
    public class LeaveBalanceDto
    {
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal TotalDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal AvailableDays { get; set; }
        public decimal CarryForwardDays { get; set; }
        public string? Color { get; set; }
    }
}
