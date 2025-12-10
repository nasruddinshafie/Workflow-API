using workflowAPI.Data.UnitOfWork;
using workflowAPI.Models.Entities;
using workflowAPI.Models.Identity;

namespace workflowAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(
            IUnitOfWork unitOfWork,
            ILogger<IdentityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            _logger.LogDebug("Getting user by ID: {UserId}", userId);
            var userEntity = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            return userEntity != null ? MapToUser(userEntity) : null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            _logger.LogDebug("Getting user by username: {Username}", username);
            var userEntity = await _unitOfWork.Users.GetByUsernameAsync(username);
            return userEntity != null ? MapToUser(userEntity) : null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogDebug("Getting user by email: {Email}", email);
            var userEntity = await _unitOfWork.Users.GetByEmailAsync(email);
            return userEntity != null ? MapToUser(userEntity) : null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogDebug("Getting all users");
            var userEntities = await _unitOfWork.Users.GetAllAsync();
            return userEntities.Select(MapToUser).ToList();
        }

        public async Task<List<User>> GetUsersByRoleAsync(Role role)
        {
            _logger.LogDebug("Getting users by role: {Role}", role);
            var userEntities = await _unitOfWork.Users.GetUsersByRoleAsync(role.ToString());
            return userEntities.Select(MapToUser).ToList();
        }

        public async Task<List<User>> GetUsersByDepartmentAsync(string department)
        {
            _logger.LogDebug("Getting users by department: {Department}", department);
            var userEntities = await _unitOfWork.Users.GetUsersByDepartmentAsync(department);
            return userEntities.Select(MapToUser).ToList();
        }

        public async Task<User?> GetManagerAsync(string userId)
        {
            _logger.LogDebug("Getting manager for user: {UserId}", userId);
            var managerEntity = await _unitOfWork.Users.GetManagerAsync(userId);
            return managerEntity != null ? MapToUser(managerEntity) : null;
        }

        public async Task<List<User>> GetDirectReportsAsync(string managerId)
        {
            _logger.LogDebug("Getting direct reports for manager: {ManagerId}", managerId);
            var userEntities = await _unitOfWork.Users.GetDirectReportsAsync(managerId);
            return userEntities.Select(MapToUser).ToList();
        }

        public async Task<bool> ValidateUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            var isValid = user != null && user.IsActive;

            _logger.LogDebug(
                "User validation for {UserId}: {IsValid}",
                userId,
                isValid);

            return isValid;
        }

        public async Task<bool> HasRoleAsync(string userId, Role role)
        {
            var hasRole = await _unitOfWork.Users.HasRoleAsync(userId, role.ToString());

            _logger.LogDebug(
                "Role check for user {UserId}, role {Role}: {HasRole}",
                userId,
                role,
                hasRole);

            return hasRole;
        }

        private User MapToUser(UserEntity entity)
        {
            return new User
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                FullName = entity.FullName,
                Department = entity.Department,
                ManagerId = entity.ManagerId,
                IsActive = entity.IsActive,
                Roles = entity.UserRoles
                    .Select(ur => Enum.Parse<Role>(ur.Role.RoleName))
                    .ToList()
            };
        }
    }
}
