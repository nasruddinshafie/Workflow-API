using workflowAPI.Models.DTOs;
using workflowAPI.Models.Entities;

namespace workflowAPI.Services
{
    public interface ILeaveService
    {
        Task<LeaveRequestEntity> CreateLeaveRequestAsync(LeaveRequestDto dto);
        Task<LeaveRequestEntity?> GetLeaveRequestAsync(string leaveRequestId);
        Task<LeaveRequestEntity?> GetLeaveRequestByIdAsync(int id);
        Task<List<UserLeaveDto>> GetUserLeavesAsync(string userId, int? year = null);
        Task<List<LeaveRequestEntity>> GetPendingApprovalsAsync(string approverId);
        Task<List<LeaveRequestEntity>> GetPendingLeavesAsync();
        Task UpdateLeaveStatusAsync(string leaveRequestId, LeaveRequestStatus status);
        Task AddApprovalAsync(string leaveRequestId, string approverId, string approverRole, ApprovalAction action, string? comments);
        Task CancelLeaveAsync(string leaveRequestId, string userId);
        Task<bool> ValidateLeaveRequestAsync(LeaveRequestDto dto);
        Task SyncWithWorkflowAsync(string leaveRequestId, string workflowProcessId, string currentState);
    }
}
