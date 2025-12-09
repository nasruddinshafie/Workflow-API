using workflowAPI.Models.Identity;

namespace workflowAPI.Data
{
    public static class DummyIdentityData
    {
        private static readonly List<User> _users = new();

        static DummyIdentityData()
        {
            InitializeUsers();
        }

        private static void InitializeUsers()
        {
            _users.AddRange(new[]
            {
                // Employees
                new User
                {
                    Id = "emp001",
                    Username = "john.doe",
                    Email = "john.doe@company.com",
                    FullName = "John Doe",
                    Department = "Engineering",
                    Roles = new List<Role> { Role.Employee },
                    ManagerId = "mgr001",
                    IsActive = true
                },
                new User
                {
                    Id = "emp002",
                    Username = "jane.smith",
                    Email = "jane.smith@company.com",
                    FullName = "Jane Smith",
                    Department = "Engineering",
                    Roles = new List<Role> { Role.Employee },
                    ManagerId = "mgr001",
                    IsActive = true
                },
                new User
                {
                    Id = "emp003",
                    Username = "bob.johnson",
                    Email = "bob.johnson@company.com",
                    FullName = "Bob Johnson",
                    Department = "Sales",
                    Roles = new List<Role> { Role.Employee },
                    ManagerId = "mgr002",
                    IsActive = true
                },
                new User
                {
                    Id = "emp004",
                    Username = "alice.williams",
                    Email = "alice.williams@company.com",
                    FullName = "Alice Williams",
                    Department = "Finance",
                    Roles = new List<Role> { Role.Employee },
                    ManagerId = "mgr003",
                    IsActive = true
                },

                // Managers
                new User
                {
                    Id = "mgr001",
                    Username = "michael.brown",
                    Email = "michael.brown@company.com",
                    FullName = "Michael Brown",
                    Department = "Engineering",
                    Roles = new List<Role> { Role.Employee, Role.Manager },
                    ManagerId = "dir001",
                    IsActive = true
                },
                new User
                {
                    Id = "mgr002",
                    Username = "sarah.davis",
                    Email = "sarah.davis@company.com",
                    FullName = "Sarah Davis",
                    Department = "Sales",
                    Roles = new List<Role> { Role.Employee, Role.Manager },
                    ManagerId = "dir001",
                    IsActive = true
                },
                new User
                {
                    Id = "mgr003",
                    Username = "david.miller",
                    Email = "david.miller@company.com",
                    FullName = "David Miller",
                    Department = "Finance",
                    Roles = new List<Role> { Role.Employee, Role.Manager, Role.FinanceManager },
                    ManagerId = "dir001",
                    IsActive = true
                },

                // HR Manager
                new User
                {
                    Id = "hr001",
                    Username = "emma.wilson",
                    Email = "emma.wilson@company.com",
                    FullName = "Emma Wilson",
                    Department = "Human Resources",
                    Roles = new List<Role> { Role.Employee, Role.Manager, Role.HRManager },
                    ManagerId = "dir001",
                    IsActive = true
                },

                // Director
                new User
                {
                    Id = "dir001",
                    Username = "robert.taylor",
                    Email = "robert.taylor@company.com",
                    FullName = "Robert Taylor",
                    Department = "Executive",
                    Roles = new List<Role> { Role.Employee, Role.Manager, Role.Director },
                    ManagerId = null,
                    IsActive = true
                },

                // Administrator
                new User
                {
                    Id = "admin001",
                    Username = "admin",
                    Email = "admin@company.com",
                    FullName = "System Administrator",
                    Department = "IT",
                    Roles = new List<Role> { Role.Administrator },
                    ManagerId = null,
                    IsActive = true
                }
            });
        }

        public static List<User> GetAllUsers() => _users;

        public static User? GetUserById(string userId)
        {
            return _users.FirstOrDefault(u => u.Id == userId);
        }

        public static User? GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public static User? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public static List<User> GetUsersByRole(Role role)
        {
            return _users.Where(u => u.Roles.Contains(role)).ToList();
        }

        public static List<User> GetUsersByDepartment(string department)
        {
            return _users.Where(u =>
                u.Department.Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static User? GetManager(string userId)
        {
            var user = GetUserById(userId);
            if (user?.ManagerId == null)
                return null;

            return GetUserById(user.ManagerId);
        }

        public static List<User> GetDirectReports(string managerId)
        {
            return _users.Where(u => u.ManagerId == managerId).ToList();
        }

        public static bool IsManager(string userId)
        {
            var user = GetUserById(userId);
            return user?.Roles.Contains(Role.Manager) == true;
        }

        public static bool IsHRManager(string userId)
        {
            var user = GetUserById(userId);
            return user?.Roles.Contains(Role.HRManager) == true;
        }

        public static bool IsFinanceManager(string userId)
        {
            var user = GetUserById(userId);
            return user?.Roles.Contains(Role.FinanceManager) == true;
        }

        // Quick reference dictionary for common test scenarios
        public static class QuickAccess
        {
            public static User Employee => GetUserById("emp001")!;
            public static User Manager => GetUserById("mgr001")!;
            public static User HRManager => GetUserById("hr001")!;
            public static User FinanceManager => GetUserById("mgr003")!;
            public static User Director => GetUserById("dir001")!;
            public static User Admin => GetUserById("admin001")!;
        }
    }
}
