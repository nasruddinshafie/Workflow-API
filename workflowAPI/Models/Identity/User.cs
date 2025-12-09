namespace workflowAPI.Models.Identity
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = new();
        public string? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
