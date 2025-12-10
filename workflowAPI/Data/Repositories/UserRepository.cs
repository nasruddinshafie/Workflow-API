using Microsoft.EntityFrameworkCore;
using workflowAPI.Models.Entities;
using workflowAPI.Models.Identity;

namespace workflowAPI.Data.Repositories
{
    public class UserRepository : Repository<UserEntity>, IUserRepository
    {
        public UserRepository(WorkflowDbContext context) : base(context)
        {
        }

        public async Task<UserEntity?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity?> GetUserWithRolesAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<UserEntity>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == roleName))
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetUsersByDepartmentAsync(string department)
        {
            return await _context.Users
                .Where(u => u.Department == department && u.IsActive)
                .ToListAsync();
        }

        public async Task<UserEntity?> GetManagerAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Manager)
                    .ThenInclude(m => m != null ? m.UserRoles : null)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Manager;
        }

        public async Task<List<UserEntity>> GetDirectReportsAsync(string managerId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .ToListAsync();
        }

        public async Task<List<Role>> GetUserRolesAsync(string userId)
        {
            var user = await GetUserWithRolesAsync(userId);
            if (user == null) return new List<Role>();

            return user.UserRoles
                .Select(ur => Enum.Parse<Role>(ur.Role.RoleName))
                .ToList();
        }

        public async Task<bool> HasRoleAsync(string userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.RoleName == roleName);
        }
    }
}
