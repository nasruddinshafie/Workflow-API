using Microsoft.EntityFrameworkCore;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Entities;

namespace workflowAPI.Data.Repositories
{
    public class LeaveRepository : Repository<LeaveRequestEntity>, ILeaveRepository
    {
        public LeaveRepository(WorkflowDbContext context) : base(context)
        {
        }

        public async Task<LeaveRequestEntity?> GetByLeaveRequestIdAsync(string leaveRequestId)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Approvals)
                .FirstOrDefaultAsync(lr => lr.LeaveRequestId == leaveRequestId);
        }

        public async Task<LeaveRequestEntity?> GetByWorkflowProcessIdAsync(string processId)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .FirstOrDefaultAsync(lr => lr.WorkflowProcessId == processId);
        }

        public async Task<List<UserLeaveDto>> GetUserLeavesAsync(string userId, int? year = null)
        {
            var query = _context.LeaveRequests
                .Where(lr => lr.UserId == userId);

            if (year.HasValue)
            {
                query = query.Where(lr => lr.StartDate.Year == year.Value);
            }

            return await query
                .OrderByDescending(lr => lr.CreatedDate)
                .Select(lr => new UserLeaveDto
                {
                    Id = lr.Id,
                    LeaveRequestId = lr.LeaveRequestId,
                    UserId = lr.UserId,
                    UserFullName = lr.User.FullName,
                    LeaveTypeId = lr.LeaveTypeId,
                    LeaveTypeName = lr.LeaveType.Name,
                    LeaveTypeCode = lr.LeaveType.Code,
                    LeaveTypeColor = lr.LeaveType.Color,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    TotalDays = lr.TotalDays,
                    Reason = lr.Reason,
                    SelectedApproverId = lr.SelectedApproverId,
                    Status = lr.Status.ToString(),
                    CurrentWorkflowState = lr.CurrentWorkflowState,
                    WorkflowProcessId = lr.WorkflowProcessId,
                    SubmittedDate = lr.SubmittedDate,
                    ApprovedDate = lr.ApprovedDate,
                    RejectedDate = lr.RejectedDate,
                    CreatedDate = lr.CreatedDate,
                    Approvals = lr.Approvals.Select(a => new LeaveApprovalInfoDto
                    {
                        Id = a.Id,
                        ApproverId = a.ApproverId,
                        ApproverFullName = a.Approver.FullName,
                        ApproverRole = a.ApproverRole,
                        Action = a.Action.ToString(),
                        Comments = a.Comments,
                        ActionDate = a.ActionDate
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<LeaveRequestEntity>> GetPendingLeavesAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                
                .OrderBy(lr => lr.SubmittedDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequestEntity>> GetPendingLeavesByApproverAsync(string approverId)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.SelectedApprover)
                .Include(lr => lr.Approvals)
                .Where(lr => lr.SelectedApproverId == approverId &&
                           (lr.Status == LeaveRequestStatus.LeaveRequestCreated || lr.Status == LeaveRequestStatus.ManagerSigning))
                .OrderBy(lr => lr.SubmittedDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequestEntity>> GetLeavesByStatusAsync(LeaveRequestStatus status)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Status == status)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequestEntity>> GetLeavesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Where(lr => (lr.StartDate >= startDate && lr.StartDate <= endDate) ||
                           (lr.EndDate >= startDate && lr.EndDate <= endDate) ||
                           (lr.StartDate <= startDate && lr.EndDate >= endDate))
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(string userId, DateTime startDate, DateTime endDate, int? excludeLeaveId = null)
        {
            var query = _context.LeaveRequests
                .Where(lr => lr.UserId == userId &&
                           lr.Status != LeaveRequestStatus.Rejected &&
                           lr.Status != LeaveRequestStatus.Cancelled &&
                           ((lr.StartDate >= startDate && lr.StartDate <= endDate) ||
                            (lr.EndDate >= startDate && lr.EndDate <= endDate) ||
                            (lr.StartDate <= startDate && lr.EndDate >= endDate)));

            if (excludeLeaveId.HasValue)
            {
                query = query.Where(lr => lr.Id != excludeLeaveId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<LeaveRequestEntity?> GetLeaveWithApprovalsAsync(int leaveId)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Approvals)
                    .ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(lr => lr.Id == leaveId);
        }

        public async Task AddApprovalAsync(LeaveApprovalEntity approval)
        {
            await _context.LeaveApprovals.AddAsync(approval);
        }
    }
}
