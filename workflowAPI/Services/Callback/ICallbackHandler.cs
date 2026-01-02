using workflowAPI.Models.Callbacks.Events;
using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;

namespace workflowAPI.Services.Callback
{
    /// <summary>
    /// Handles workflow callback events
    /// </summary>
    public interface ICallbackHandler
    {
        Task HandleStatusChangedAsync(ProcessStatusChangedRequest callback);
        Task HandleActivityChangedAsync(ProcessActivityChangedRequest callback);
        Task<ExecuteActionResponse> HandleActionExecutedAsync(ExecuteActionRequest callback);
        Task<GetActionResponse> GetAvailableActionsAsync(string schemeCode);
        Task<GetConditionResponse> GetAvailableConditionAsync(string schemeCode);
        Task<ExecuteConditionResponse> HandleConditionExecutedAsync(ExecuteConditionRequest callback);
        Task HandleProcessLogAsync(ProcessLogsRequest request);
    }
}
