using Microsoft.EntityFrameworkCore;
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

        public async Task<List<LeaveRequestEntity>> GetUserLeavesAsync(string userId, int? year = null)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Approvals)
                .Where(lr => lr.UserId == userId);

            if (year.HasValue)
            {
                query = query.Where(lr => lr.StartDate.Year == year.Value);
            }

            return await query
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequestEntity>> GetPendingLeavesAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Status == LeaveRequestStatus.Pending ||
                           lr.Status == LeaveRequestStatus.ManagerApproved)
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
