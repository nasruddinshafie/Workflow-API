namespace workflowAPI.Models.Entities
{
    public class LeaveBalanceEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public int Year { get; set; }
        public decimal TotalDays { get; set; }
        public decimal UsedDays { get; set; } = 0;
        public decimal PendingDays { get; set; } = 0;
        public decimal? CarryForwardDays { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Computed property (will be configured as computed column in database)
        public decimal AvailableDays { get; private set; }

        // Navigation properties
        public virtual UserEntity User { get; set; } = null!;
        public virtual LeaveTypeEntity LeaveType { get; set; } = null!;
    }
}
