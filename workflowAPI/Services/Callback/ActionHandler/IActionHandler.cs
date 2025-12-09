using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;

namespace workflowAPI.Services.Callback.ActionHandler
{
    /// <summary>
    /// Interface for handling workflow-specific actions
    /// Each workflow type gets its own action handler (SRP)
    /// </summary>
    public interface IActionHandler
    {
        /// <summary>
        /// Workflow type this handler supports
        /// </summary>
        string WorkflowType { get; }

        /// <summary>
        /// Get list of available actions for this workflow type
        /// </summary>
        Task<GetActionResponse> GetActionsAsync(string schemeCode);

        /// <summary>
        /// Execute action for this workflow type
        /// </summary>
        Task<ExecuteActionResponse> ExecuteActionAsync(ExecuteActionRequest callback);

        /// <summary>
        /// Check if this handler can handle the given workflow type
        /// </summary>
        bool CanHandle(string workflowType);
    }
}
