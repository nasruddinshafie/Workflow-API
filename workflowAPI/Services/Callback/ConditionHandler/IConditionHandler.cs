using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;

namespace workflowAPI.Services.Callback.ConditionHandler
{
    /// <summary>
    /// Interface for handling workflow-specific condition
    /// Each workflow type gets its own condition handler (SRP)
    /// </summary>
    public interface IConditionHandler
    {
        /// <summary>
        /// Workflow type this handler supports
        /// </summary>
        string WorkflowType { get; }


        /// <summary>
        /// Get list of available condition for this workflow type
        /// </summary>
        Task<GetConditionResponse> GetConditionsAsync(string schemeCode);


        /// <summary>
        /// Execute condition for this workflow type
        /// </summary>
        Task<ExecuteConditionResponse> ExecuteConditionAsync(ExecuteConditionRequest callback);

        /// <summary>
        /// Check if this handler can handle the given workflow type
        /// </summary>
        bool CanHandle(string workflowType);

    }
}
