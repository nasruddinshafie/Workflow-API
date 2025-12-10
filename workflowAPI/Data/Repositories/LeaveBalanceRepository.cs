using Microsoft.EntityFrameworkCore;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Repositories
{
    public class LeaveBalanceRepository : Repository<LeaveBalanceEntity>, ILeaveBalanceRepository
    {
        public LeaveBalanceRepository(WorkflowDbContext context) : base(context)
        {
        }

        public async Task<LeaveBalanceEntity?> GetBalanceAsync(string userId, int leaveTypeId, int year)
        {
            return await _context.LeaveBalances
                .Include(lb => lb.LeaveType)
                .FirstOrDefaultAsync(lb => lb.UserId == userId &&
                                          lb.LeaveTypeId == leaveTypeId &&
                                          lb.Year == year);
        }

        public async Task<LeaveBalanceEntity?> GetBalanceByCodeAsync(string userId, string leaveTypeCode, int year)
        {
            return await _context.LeaveBalances
                .Include(lb => lb.LeaveType)
                .FirstOrDefaultAsync(lb => lb.UserId == userId &&
                                          lb.LeaveType.Code == leaveTypeCode &&
                                          lb.Year == year);
        }

        public async Task<List<LeaveBalanceEntity>> GetUserBalancesAsync(string userId, int year)
        {
            return await _context.LeaveBalances
                .Include(lb => lb.LeaveType)
                .Where(lb => lb.UserId == userId && lb.Year == year)
                .OrderBy(lb => lb.LeaveType.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> HasSufficientBalanceAsync(string userId, int leaveTypeId, int year, decimal daysRequired)
        {
            var balance = await GetBalanceAsync(userId, leaveTypeId, year);
            if (balance == null) return false;

            return balance.AvailableDays >= daysRequired;
        }

        public async Task UpdateBalanceAsync(string userId, int leaveTypeId, int year, decimal usedDays, decimal pendingDays)
        {
            var balance = await GetBalanceAsync(userId, leaveTypeId, year);
            if (balance == null)
            {
                throw new InvalidOperationException($"Leave balance not found for user {userId}, leave type {leaveTypeId}, year {year}");
            }

            balance.UsedDays = usedDays;
            balance.PendingDays = pendingDays;
            balance.UpdatedDate = DateTime.UtcNow;

            _context.LeaveBalances.Update(balance);
        }

        public async Task<decimal> GetAvailableBalanceAsync(string userId, int leaveTypeId, int year)
        {
            var balance = await GetBalanceAsync(userId, leaveTypeId, year);
            return balance?.AvailableDays ?? 0;
        }

        public async Task InitializeBalancesForUserAsync(string userId, int year)
        {
            var leaveTypes = await _context.LeaveTypes
                .Where(lt => lt.IsActive && lt.DefaultDaysPerYear > 0)
                .ToListAsync();

            var balances = leaveTypes.Select(lt => new LeaveBalanceEntity
            {
                UserId = userId,
                LeaveTypeId = lt.Id,
                Year = year,
                TotalDays = lt.DefaultDaysPerYear,
                UsedDays = 0,
                PendingDays = 0,
                CarryForwardDays = 0
            });

            await _context.LeaveBalances.AddRangeAsync(balances);
        }
    }
}
