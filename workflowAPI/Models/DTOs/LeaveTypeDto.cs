namespace workflowAPI.Models.DTOs
{
    public class LeaveTypeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DefaultDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }
}
