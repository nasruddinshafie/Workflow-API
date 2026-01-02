using workflowAPI.Models.Requests;
using workflowAPI.Models.Responses;

namespace workflowAPI.Services
{
    public interface IWorkflowService
    {
        Task<string> CreateInstanceAsync(
           string workflowType,
           string identityId,
           string leaveRequestId,
           Dictionary<string, object> parameters);

        Task<GetInstanceInfoResponse> GetInstanceAsync(string processId);

        Task<CommandsResponse> GetAvailableCommandsAsync(
            string processId,
            string identityId);

        Task ExecuteCommandAsync(ExecuteCommandRequest request);

        Task<List<WorkflowInstanceResponse>> GetInstancesBySchemeAsync(
            string schemeCode,
            int limit = 100);

        Task WriteLogAsync(string message, Dictionary<string, object>? additionalParameters = null);

    }
}
