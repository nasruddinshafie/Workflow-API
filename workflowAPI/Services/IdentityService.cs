using workflowAPI.Data;
using workflowAPI.Models.Identity;

namespace workflowAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(ILogger<IdentityService> logger)
        {
            _logger = logger;
        }

        public User? GetUserById(string userId)
        {
            _logger.LogDebug("Getting user by ID: {UserId}", userId);
            return DummyIdentityData.GetUserById(userId);
        }

        public User? GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user by username: {Username}", username);
            return DummyIdentityData.GetUserByUsername(username);
        }

        public User? GetUserByEmail(string email)
        {
            _logger.LogDebug("Getting user by email: {Email}", email);
            return DummyIdentityData.GetUserByEmail(email);
        }

        public List<User> GetAllUsers()
        {
            _logger.LogDebug("Getting all users");
            return DummyIdentityData.GetAllUsers();
        }

        public List<User> GetUsersByRole(Role role)
        {
            _logger.LogDebug("Getting users by role: {Role}", role);
            return DummyIdentityData.GetUsersByRole(role);
        }

        public List<User> GetUsersByDepartment(string department)
        {
            _logger.LogDebug("Getting users by department: {Department}", department);
            return DummyIdentityData.GetUsersByDepartment(department);
        }

        public User? GetManager(string userId)
        {
            _logger.LogDebug("Getting manager for user: {UserId}", userId);
            return DummyIdentityData.GetManager(userId);
        }

        public List<User> GetDirectReports(string managerId)
        {
            _logger.LogDebug("Getting direct reports for manager: {ManagerId}", managerId);
            return DummyIdentityData.GetDirectReports(managerId);
        }

        public bool ValidateUser(string userId)
        {
            var user = GetUserById(userId);
            var isValid = user != null && user.IsActive;

            _logger.LogDebug(
                "User validation for {UserId}: {IsValid}",
                userId,
                isValid);

            return isValid;
        }

        public bool HasRole(string userId, Role role)
        {
            var user = GetUserById(userId);
            var hasRole = user?.Roles.Contains(role) == true;

            _logger.LogDebug(
                "Role check for user {UserId}, role {Role}: {HasRole}",
                userId,
                role,
                hasRole);

            return hasRole;
        }
    }
}
