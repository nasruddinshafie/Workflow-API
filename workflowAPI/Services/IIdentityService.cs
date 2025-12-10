using workflowAPI.Models.Identity;

namespace workflowAPI.Services
{
    public interface IIdentityService
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(Role role);
        Task<List<User>> GetUsersByDepartmentAsync(string department);
        Task<User?> GetManagerAsync(string userId);
        Task<List<User>> GetDirectReportsAsync(string managerId);
        Task<bool> ValidateUserAsync(string userId);
        Task<bool> HasRoleAsync(string userId, Role role);
    }
}
