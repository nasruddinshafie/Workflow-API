using workflowAPI.Models.DTOs;

namespace workflowAPI.Services
{
    public interface ILeaveBalanceService
    {
        Task<List<LeaveBalanceDto>> GetUserBalancesAsync(string userId, int? year = null);
        Task<LeaveBalanceDto?> GetBalanceAsync(string userId, string leaveTypeCode, int? year = null);
        Task<bool> HasSufficientBalanceAsync(string userId, string leaveTypeCode, int year, decimal daysRequired);
        Task ReservePendingDaysAsync(string userId, string leaveTypeCode, int year, decimal days);

        Task ConfirmLeaveAsync(string userId, string leaveTypeCode, int year, decimal days);
        Task ReleasePendingDaysAsync(string userId, string leaveTypeCode, int year, decimal days);
        Task InitializeBalancesForYearAsync(int year);
    }
}
