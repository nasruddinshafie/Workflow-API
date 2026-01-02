using workflowAPI.Models.DTOs;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Repositories
{
    public interface ILeaveRepository : IRepository<LeaveRequestEntity>
    {
        Task<LeaveRequestEntity?> GetByLeaveRequestIdAsync(string leaveRequestId);
        Task<LeaveRequestEntity?> GetByWorkflowProcessIdAsync(string processId);
        Task<List<UserLeaveDto>> GetUserLeavesAsync(string userId, int? year = null);
        Task<List<LeaveRequestEntity>> GetPendingLeavesAsync();
        Task<List<LeaveRequestEntity>> GetPendingLeavesByApproverAsync(string approverId);
        Task<List<LeaveRequestEntity>> GetLeavesByStatusAsync(LeaveRequestStatus status);
        Task<List<LeaveRequestEntity>> GetLeavesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> HasOverlappingLeaveAsync(string userId, DateTime startDate, DateTime endDate, int? excludeLeaveId = null);
        Task<LeaveRequestEntity?> GetLeaveWithApprovalsAsync(int leaveId);
        Task AddApprovalAsync(LeaveApprovalEntity approval);
    }
}
