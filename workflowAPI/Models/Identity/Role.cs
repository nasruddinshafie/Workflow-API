namespace workflowAPI.Models.Identity
{
    public enum Role
    {
        Employee,
        Manager,
        HRManager,
        FinanceManager,
        Director,
        Administrator
    }

    public static class RoleExtensions
    {
        public static string ToDisplayName(this Role role)
        {
            return role switch
            {
                Role.Employee => "Employee",
                Role.Manager => "Manager",
                Role.HRManager => "HR Manager",
                Role.FinanceManager => "Finance Manager",
                Role.Director => "Director",
                Role.Administrator => "Administrator",
                _ => role.ToString()
            };
        }

        public static bool HasRole(this User user, Role role)
        {
            return user.Roles.Contains(role);
        }

        public static bool HasAnyRole(this User user, params Role[] roles)
        {
            return user.Roles.Any(r => roles.Contains(r));
        }
    }
}
