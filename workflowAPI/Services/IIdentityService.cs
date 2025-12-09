using workflowAPI.Models.Identity;

namespace workflowAPI.Services
{
    public interface IIdentityService
    {
        User? GetUserById(string userId);
        User? GetUserByUsername(string username);
        User? GetUserByEmail(string email);
        List<User> GetAllUsers();
        List<User> GetUsersByRole(Role role);
        List<User> GetUsersByDepartment(string department);
        User? GetManager(string userId);
        List<User> GetDirectReports(string managerId);
        bool ValidateUser(string userId);
        bool HasRole(string userId, Role role);
    }
}
