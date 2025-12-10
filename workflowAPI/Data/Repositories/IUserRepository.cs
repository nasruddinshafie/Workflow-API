using workflowAPI.Models.Entities;
using workflowAPI.Models.Identity;

namespace workflowAPI.Data.Repositories
{
    public interface IUserRepository : IRepository<UserEntity>
    {
        Task<UserEntity?> GetByUsernameAsync(string username);
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity?> GetUserWithRolesAsync(string userId);
        Task<List<UserEntity>> GetUsersByRoleAsync(string roleName);
        Task<List<UserEntity>> GetUsersByDepartmentAsync(string department);
        Task<UserEntity?> GetManagerAsync(string userId);
        Task<List<UserEntity>> GetDirectReportsAsync(string managerId);
        Task<List<Role>> GetUserRolesAsync(string userId);
        Task<bool> HasRoleAsync(string userId, string roleName);
    }
}
