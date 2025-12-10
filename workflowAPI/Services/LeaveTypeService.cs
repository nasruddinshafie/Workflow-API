using Microsoft.EntityFrameworkCore;
using workflowAPI.Data;
using workflowAPI.Models.Entities;

namespace workflowAPI.Services
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly WorkflowDbContext _context;
        private readonly ILogger<LeaveTypeService> _logger;

        public LeaveTypeService(
            WorkflowDbContext context,
            ILogger<LeaveTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LeaveTypeEntity>> GetAllActiveLeaveTypesAsync()
        {
            _logger.LogInformation("Getting all active leave types");

            return await _context.LeaveTypes
                .Where(lt => lt.IsActive)
                .OrderBy(lt => lt.SortOrder)
                .ThenBy(lt => lt.Name)
                .ToListAsync();
        }

        public async Task<LeaveTypeEntity?> GetLeaveTypeByCodeAsync(string code)
        {
            _logger.LogInformation("Getting leave type by code: {Code}", code);

            return await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.Code == code && lt.IsActive);
        }

        public async Task<LeaveTypeEntity?> GetLeaveTypeByIdAsync(int id)
        {
            _logger.LogInformation("Getting leave type by ID: {Id}", id);

            return await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.Id == id);
        }
    }
}
