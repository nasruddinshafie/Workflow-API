using workflowAPI.Models.Requests;
using workflowAPI.Models.Responses;

namespace workflowAPI.Services
{
    public interface IWorkflowService
    {
        Task<CreateInstanceResponse> CreateInstanceAsync(
           string workflowType,
           string identityId,
           Dictionary<string, object> parameters);

        Task<WorkflowInstanceResponse> GetInstanceAsync(string processId);

        Task<CommandsResponse> GetAvailableCommandsAsync(
            string processId,
            string identityId);

        Task ExecuteCommandAsync(ExecuteCommandRequest request);

        Task<List<WorkflowInstanceResponse>> GetInstancesBySchemeAsync(
            string schemeCode,
            int limit = 100);
    }
}
