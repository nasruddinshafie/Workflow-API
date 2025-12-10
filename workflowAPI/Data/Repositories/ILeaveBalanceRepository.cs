using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Repositories
{
    public interface ILeaveBalanceRepository : IRepository<LeaveBalanceEntity>
    {
        Task<LeaveBalanceEntity?> GetBalanceAsync(string userId, int leaveTypeId, int year);
        Task<LeaveBalanceEntity?> GetBalanceByCodeAsync(string userId, string leaveTypeCode, int year);
        Task<List<LeaveBalanceEntity>> GetUserBalancesAsync(string userId, int year);
        Task<bool> HasSufficientBalanceAsync(string userId, int leaveTypeId, int year, decimal daysRequired);
        Task UpdateBalanceAsync(string userId, int leaveTypeId, int year, decimal usedDays, decimal pendingDays);
        Task<decimal> GetAvailableBalanceAsync(string userId, int leaveTypeId, int year);
        Task InitializeBalancesForUserAsync(string userId, int year);
    }
}
