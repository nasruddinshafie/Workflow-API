using Microsoft.EntityFrameworkCore;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<RoleEntity> Roles { get; set; } = null!;
        public DbSet<UserRoleEntity> UserRoles { get; set; } = null!;
        public DbSet<LeaveTypeEntity> LeaveTypes { get; set; } = null!;
        public DbSet<LeaveBalanceEntity> LeaveBalances { get; set; } = null!;
        public DbSet<LeaveRequestEntity> LeaveRequests { get; set; } = null!;
        public DbSet<LeaveApprovalEntity> LeaveApprovals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from separate files
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is UserEntity user)
                    user.UpdatedDate = DateTime.UtcNow;
                else if (entry.Entity is LeaveRequestEntity request)
                    request.UpdatedDate = DateTime.UtcNow;
                else if (entry.Entity is LeaveBalanceEntity balance)
                    balance.UpdatedDate = DateTime.UtcNow;
                else if (entry.Entity is LeaveTypeEntity leaveType)
                    leaveType.UpdatedDate = DateTime.UtcNow;
            }
        }
    }
}
