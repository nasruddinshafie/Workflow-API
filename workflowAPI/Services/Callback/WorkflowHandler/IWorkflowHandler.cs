using workflowAPI.Models.Callbacks.Events;

namespace workflowAPI.Services.Callback.WorkflowHandler
{
    public interface IWorkflowHandler
    {
        /// <summary>
        /// Workflow type this handler supports
        /// </summary>
        string WorkflowType { get; }

        /// <summary>
        /// Handle status changed callback for this workflow type
        /// </summary>
        Task HandleStatusChangedAsync(ProcessStatusChangedRequest callback);


        /// <summary>
        /// Handle activity changed callback for this workflow type
        /// </summary>
        Task HandleActivityChangedAsync(ProcessActivityChangedRequest callback);

        /// <summary>
        /// Check if this handler can handle the given workflow type
        /// </summary>
        bool CanHandle(string workflowType);


    }
}
