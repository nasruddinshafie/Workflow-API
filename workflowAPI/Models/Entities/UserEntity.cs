namespace workflowAPI.Models.Entities
{
    public class UserEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual UserEntity? Manager { get; set; }
        public virtual ICollection<UserEntity> DirectReports { get; set; } = new List<UserEntity>();
        public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
        public virtual ICollection<LeaveBalanceEntity> LeaveBalances { get; set; } = new List<LeaveBalanceEntity>();
        public virtual ICollection<LeaveRequestEntity> LeaveRequests { get; set; } = new List<LeaveRequestEntity>();
    }
}
