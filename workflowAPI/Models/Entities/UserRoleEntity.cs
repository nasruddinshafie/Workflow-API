namespace workflowAPI.Models.Entities
{
    public class UserRoleEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }

        // Navigation properties
        public virtual UserEntity User { get; set; } = null!;
        public virtual RoleEntity Role { get; set; } = null!;
    }
}
