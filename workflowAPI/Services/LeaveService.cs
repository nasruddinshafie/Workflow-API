using Microsoft.EntityFrameworkCore;
using workflowAPI.Data.UnitOfWork;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Entities;

namespace workflowAPI.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(
            IUnitOfWork unitOfWork,
            ILogger<LeaveService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LeaveRequestEntity> CreateLeaveRequestAsync(LeaveRequestDto dto)
        {
            _logger.LogInformation("Creating leave request for user {UserId}", dto.EmployeeId);

            // Get leave type
            var leaveType = await _unitOfWork.LeaveBalances.FirstOrDefaultAsync(
                lt => lt.LeaveType.Code == dto.LeaveType);

            if (leaveType == null)
            {
                throw new InvalidOperationException($"Leave type '{dto.LeaveType}' not found");
            }

            // Calculate total days
            var totalDays = (decimal)(dto.EndDate - dto.StartDate).Days + 1;

            // Create leave request entity
            var leaveRequest = new LeaveRequestEntity
            {
                LeaveRequestId = Guid.NewGuid().ToString(),
                UserId = dto.EmployeeId,
                LeaveTypeId = leaveType.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Reason = dto.Reason,
                SelectedApproverId = dto.SelectedApproverId,
                Status = LeaveRequestStatus.LeaveRequestCreated,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Leaves.AddAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request {LeaveRequestId} created successfully", leaveRequest.LeaveRequestId);

            return leaveRequest;
        }

        public async Task<LeaveRequestEntity?> GetLeaveRequestAsync(string leaveRequestId)
        {
            return await _unitOfWork.Leaves.GetByLeaveRequestIdAsync(leaveRequestId);
        }

        public async Task<LeaveRequestEntity?> GetLeaveRequestByIdAsync(int id)
        {
            return await _unitOfWork.Leaves.GetByIdAsync(id);
        }

        public async Task<List<UserLeaveDto>> GetUserLeavesAsync(string userId, int? year = null)
        {
            return await _unitOfWork.Leaves.GetUserLeavesAsync(userId, year);
        }

        public async Task<List<LeaveRequestEntity>> GetPendingApprovalsAsync(string approverId)
        {
            return await _unitOfWork.Leaves.GetPendingLeavesByApproverAsync(approverId);
        }

        public async Task<List<LeaveRequestEntity>> GetPendingLeavesAsync()
        {
            return await _unitOfWork.Leaves.GetPendingLeavesAsync();
        }

        public async Task UpdateLeaveStatusAsync(string leaveRequestId, LeaveRequestStatus status)
        {
            var leaveRequest = await GetLeaveRequestAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                throw new InvalidOperationException($"Leave request {leaveRequestId} not found");
            }

            leaveRequest.Status = status;
            leaveRequest.UpdatedDate = DateTime.UtcNow;

            switch (status)
            {
                case LeaveRequestStatus.LeaveRequestCreated:
                    leaveRequest.SubmittedDate = DateTime.UtcNow;
                    break;
                case LeaveRequestStatus.Approved:
                    leaveRequest.ApprovedDate = DateTime.UtcNow;
                    break;
                case LeaveRequestStatus.Rejected:
                    leaveRequest.RejectedDate = DateTime.UtcNow;
                    break;
                case LeaveRequestStatus.Cancelled:
                    leaveRequest.CancelledDate = DateTime.UtcNow;
                    break;
            }

            _unitOfWork.Leaves.Update(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request {LeaveRequestId} status updated to {Status}", leaveRequestId, status);
        }

        public async Task AddApprovalAsync(string leaveRequestId, string approverId, string approverRole, ApprovalAction action, string? comments)
        {
            var leaveRequest = await GetLeaveRequestAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                throw new InvalidOperationException($"Leave request {leaveRequestId} not found");
            }

            var approval = new LeaveApprovalEntity
            {
                LeaveRequestId = leaveRequest.Id,
                ApproverId = approverId,
                ApproverRole = approverRole,
                Action = action,
                Comments = comments,
                ActionDate = DateTime.UtcNow
            };

            await _unitOfWork.Leaves.AddApprovalAsync(approval);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Approval recorded for leave request {LeaveRequestId} by {ApproverId} ({ApproverRole}): {Action}",
                leaveRequestId, approverId, approverRole, action);
        }

        public async Task CancelLeaveAsync(string leaveRequestId, string userId)
        {
            var leaveRequest = await GetLeaveRequestAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                throw new InvalidOperationException($"Leave request {leaveRequestId} not found");
            }

            if (leaveRequest.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own leave requests");
            }

            if (leaveRequest.Status == LeaveRequestStatus.Approved ||
                leaveRequest.Status == LeaveRequestStatus.Rejected ||
                leaveRequest.Status == LeaveRequestStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot cancel leave request in {leaveRequest.Status} status");
            }

            await UpdateLeaveStatusAsync(leaveRequestId, LeaveRequestStatus.Cancelled);

            _logger.LogInformation("Leave request {LeaveRequestId} cancelled by user {UserId}", leaveRequestId, userId);
        }

        public async Task<bool> ValidateLeaveRequestAsync(LeaveRequestDto dto)
        {
            // Validate dates
            if (dto.StartDate > dto.EndDate)
            {
                _logger.LogWarning("Invalid leave request: Start date {StartDate} is after end date {EndDate}",
                    dto.StartDate, dto.EndDate);
                return false;
            }

            if (dto.StartDate < DateTime.Today)
            {
                _logger.LogWarning("Invalid leave request: Start date {StartDate} is in the past", dto.StartDate);
                return false;
            }

            // Check for overlapping leaves
            var hasOverlap = await _unitOfWork.Leaves.HasOverlappingLeaveAsync(
                dto.EmployeeId,
                dto.StartDate,
                dto.EndDate);

            if (hasOverlap)
            {
                _logger.LogWarning("Leave request overlaps with existing leave for user {UserId}", dto.EmployeeId);
                return false;
            }

            return true;
        }

        public async Task SyncWithWorkflowAsync(string leaveRequestId, string workflowProcessId, string currentState)
        {
            var leaveRequest = await GetLeaveRequestAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                throw new InvalidOperationException($"Leave request {leaveRequestId} not found");
            }

            leaveRequest.WorkflowProcessId = workflowProcessId;
            leaveRequest.CurrentWorkflowState = currentState;
            leaveRequest.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Leaves.Update(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Leave request {LeaveRequestId} synced with workflow {WorkflowProcessId}, state: {State}",
                leaveRequestId, workflowProcessId, currentState);
        }
    }
}
