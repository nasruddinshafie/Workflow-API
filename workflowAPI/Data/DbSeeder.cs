using Microsoft.EntityFrameworkCore;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data
{
    public class DbSeeder
    {
        private readonly WorkflowDbContext _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(WorkflowDbContext context, ILogger<DbSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                await SeedRolesAsync();
                await SeedLeaveTypesAsync();
                await SeedUsersAsync();
                await SeedLeaveBalancesAsync();

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            if (await _context.Roles.AnyAsync())
            {
                _logger.LogInformation("Roles already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding roles...");

            var roles = new[]
            {
                new RoleEntity
                {
                    RoleName = "Employee",
                    DisplayName = "Employee",
                    Description = "Regular employee",
                    IsActive = true
                },
                new RoleEntity
                {
                    RoleName = "Manager",
                    DisplayName = "Manager",
                    Description = "Team manager",
                    IsActive = true
                },
                new RoleEntity
                {
                    RoleName = "HRManager",
                    DisplayName = "HR Manager",
                    Description = "Human resources manager",
                    IsActive = true
                },
                new RoleEntity
                {
                    RoleName = "FinanceManager",
                    DisplayName = "Finance Manager",
                    Description = "Finance manager",
                    IsActive = true
                },
                new RoleEntity
                {
                    RoleName = "Director",
                    DisplayName = "Director",
                    Description = "Executive director",
                    IsActive = true
                },
                new RoleEntity
                {
                    RoleName = "Administrator",
                    DisplayName = "Administrator",
                    Description = "System administrator",
                    IsActive = true
                }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} roles", roles.Length);
        }

        private async Task SeedLeaveTypesAsync()
        {
            if (await _context.LeaveTypes.AnyAsync())
            {
                _logger.LogInformation("Leave types already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding leave types...");

            var leaveTypes = new[]
            {
                new LeaveTypeEntity
                {
                    Code = "ANNUAL",
                    Name = "Annual Leave",
                    Description = "Paid annual vacation leave",
                    DefaultDaysPerYear = 21,
                    RequiresApproval = true,
                    Color = "#4CAF50",
                    SortOrder = 1,
                    IsActive = true
                },
                new LeaveTypeEntity
                {
                    Code = "SICK",
                    Name = "Sick Leave",
                    Description = "Medical/sick leave",
                    DefaultDaysPerYear = 14,
                    RequiresApproval = true,
                    Color = "#FF9800",
                    SortOrder = 2,
                    IsActive = true
                },
                new LeaveTypeEntity
                {
                    Code = "PERSONAL",
                    Name = "Personal Leave",
                    Description = "Personal time off",
                    DefaultDaysPerYear = 5,
                    RequiresApproval = true,
                    Color = "#2196F3",
                    SortOrder = 3,
                    IsActive = true
                },
                new LeaveTypeEntity
                {
                    Code = "UNPAID",
                    Name = "Unpaid Leave",
                    Description = "Leave without pay",
                    DefaultDaysPerYear = 0,
                    RequiresApproval = true,
                    Color = "#9E9E9E",
                    SortOrder = 4,
                    IsActive = true
                },
                new LeaveTypeEntity
                {
                    Code = "MATERNITY",
                    Name = "Maternity Leave",
                    Description = "Maternity leave",
                    DefaultDaysPerYear = 90,
                    RequiresApproval = true,
                    Color = "#E91E63",
                    SortOrder = 5,
                    IsActive = true
                },
                new LeaveTypeEntity
                {
                    Code = "PATERNITY",
                    Name = "Paternity Leave",
                    Description = "Paternity leave",
                    DefaultDaysPerYear = 14,
                    RequiresApproval = true,
                    Color = "#3F51B5",
                    SortOrder = 6,
                    IsActive = true
                }
            };

            await _context.LeaveTypes.AddRangeAsync(leaveTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} leave types", leaveTypes.Length);
        }

        private async Task SeedUsersAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Users already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding users...");

            // Get role entities for mapping
            var roles = await _context.Roles.ToDictionaryAsync(r => r.RoleName);

            // Create users
            var users = new[]
            {
                // Employees
                new UserEntity
                {
                    Id = "emp001",
                    Username = "john.doe",
                    Email = "john.doe@company.com",
                    FullName = "John Doe",
                    Department = "Engineering",
                    ManagerId = "mgr001",
                    IsActive = true
                },
                new UserEntity
                {
                    Id = "emp002",
                    Username = "jane.smith",
                    Email = "jane.smith@company.com",
                    FullName = "Jane Smith",
                    Department = "Engineering",
                    ManagerId = "mgr001",
                    IsActive = true
                },
                new UserEntity
                {
                    Id = "emp003",
                    Username = "bob.johnson",
                    Email = "bob.johnson@company.com",
                    FullName = "Bob Johnson",
                    Department = "Sales",
                    ManagerId = "mgr002",
                    IsActive = true
                },
                new UserEntity
                {
                    Id = "emp004",
                    Username = "alice.williams",
                    Email = "alice.williams@company.com",
                    FullName = "Alice Williams",
                    Department = "Finance",
                    ManagerId = "mgr003",
                    IsActive = true
                },

                // Managers
                new UserEntity
                {
                    Id = "mgr001",
                    Username = "michael.brown",
                    Email = "michael.brown@company.com",
                    FullName = "Michael Brown",
                    Department = "Engineering",
                    ManagerId = "dir001",
                    IsActive = true
                },
                new UserEntity
                {
                    Id = "mgr002",
                    Username = "sarah.davis",
                    Email = "sarah.davis@company.com",
                    FullName = "Sarah Davis",
                    Department = "Sales",
                    ManagerId = "dir001",
                    IsActive = true
                },
                new UserEntity
                {
                    Id = "mgr003",
                    Username = "david.miller",
                    Email = "david.miller@company.com",
                    FullName = "David Miller",
                    Department = "Finance",
                    ManagerId = "dir001",
                    IsActive = true
                },

                // HR Manager
                new UserEntity
                {
                    Id = "hr001",
                    Username = "emma.wilson",
                    Email = "emma.wilson@company.com",
                    FullName = "Emma Wilson",
                    Department = "Human Resources",
                    ManagerId = "dir001",
                    IsActive = true
                },

                // Director
                new UserEntity
                {
                    Id = "dir001",
                    Username = "robert.taylor",
                    Email = "robert.taylor@company.com",
                    FullName = "Robert Taylor",
                    Department = "Executive",
                    ManagerId = null,
                    IsActive = true
                },

                // Administrator
                new UserEntity
                {
                    Id = "admin001",
                    Username = "admin",
                    Email = "admin@company.com",
                    FullName = "System Administrator",
                    Department = "IT",
                    ManagerId = null,
                    IsActive = true
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} users", users.Length);

            // Assign roles to users
            var userRoles = new List<UserRoleEntity>
            {
                // emp001 - Employee
                new() { UserId = "emp001", RoleId = roles["Employee"].Id },

                // emp002 - Employee
                new() { UserId = "emp002", RoleId = roles["Employee"].Id },

                // emp003 - Employee
                new() { UserId = "emp003", RoleId = roles["Employee"].Id },

                // emp004 - Employee
                new() { UserId = "emp004", RoleId = roles["Employee"].Id },

                // mgr001 - Employee, Manager
                new() { UserId = "mgr001", RoleId = roles["Employee"].Id },
                new() { UserId = "mgr001", RoleId = roles["Manager"].Id },

                // mgr002 - Employee, Manager
                new() { UserId = "mgr002", RoleId = roles["Employee"].Id },
                new() { UserId = "mgr002", RoleId = roles["Manager"].Id },

                // mgr003 - Employee, Manager, FinanceManager
                new() { UserId = "mgr003", RoleId = roles["Employee"].Id },
                new() { UserId = "mgr003", RoleId = roles["Manager"].Id },
                new() { UserId = "mgr003", RoleId = roles["FinanceManager"].Id },

                // hr001 - Employee, Manager, HRManager
                new() { UserId = "hr001", RoleId = roles["Employee"].Id },
                new() { UserId = "hr001", RoleId = roles["Manager"].Id },
                new() { UserId = "hr001", RoleId = roles["HRManager"].Id },

                // dir001 - Employee, Manager, Director
                new() { UserId = "dir001", RoleId = roles["Employee"].Id },
                new() { UserId = "dir001", RoleId = roles["Manager"].Id },
                new() { UserId = "dir001", RoleId = roles["Director"].Id },

                // admin001 - Administrator
                new() { UserId = "admin001", RoleId = roles["Administrator"].Id }
            };

            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned roles to users");
        }

        private async Task SeedLeaveBalancesAsync()
        {
            if (await _context.LeaveBalances.AnyAsync())
            {
                _logger.LogInformation("Leave balances already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding leave balances...");

            var currentYear = DateTime.UtcNow.Year;
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();

            var balances = new List<LeaveBalanceEntity>();

            foreach (var user in users)
            {
                foreach (var leaveType in leaveTypes)
                {
                    if (leaveType.DefaultDaysPerYear > 0)
                    {
                        balances.Add(new LeaveBalanceEntity
                        {
                            UserId = user.Id,
                            LeaveTypeId = leaveType.Id,
                            Year = currentYear,
                            TotalDays = leaveType.DefaultDaysPerYear,
                            UsedDays = 0,
                            PendingDays = 0,
                            CarryForwardDays = 0
                        });
                    }
                }
            }

            await _context.LeaveBalances.AddRangeAsync(balances);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} leave balances for {UserCount} users", balances.Count, users.Count);
        }
    }
}
