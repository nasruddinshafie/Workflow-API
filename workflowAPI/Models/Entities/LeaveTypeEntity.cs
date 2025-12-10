namespace workflowAPI.Models.Entities
{
    public class LeaveTypeEntity
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DefaultDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Color { get; set; }
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<LeaveBalanceEntity> LeaveBalances { get; set; } = new List<LeaveBalanceEntity>();
        public virtual ICollection<LeaveRequestEntity> LeaveRequests { get; set; } = new List<LeaveRequestEntity>();
    }
}
