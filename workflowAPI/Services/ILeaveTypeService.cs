using workflowAPI.Models.Entities;

namespace workflowAPI.Services
{
    public interface ILeaveTypeService
    {
        Task<List<LeaveTypeEntity>> GetAllActiveLeaveTypesAsync();
        Task<LeaveTypeEntity?> GetLeaveTypeByCodeAsync(string code);
        Task<LeaveTypeEntity?> GetLeaveTypeByIdAsync(int id);
    }
}
