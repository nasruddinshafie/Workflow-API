using workflowAPI.Models.Callbacks;
using workflowAPI.Models.Callbacks.Events;
using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;
using workflowAPI.Services.Callback.ActionHandler;
using workflowAPI.Services.Callback.WorkflowHandler;

namespace workflowAPI.Services.Callback
{
    /// <summary>
    /// Implements workflow callback handling using Strategy Pattern
    /// SOLID Principles:
    /// - SRP: Delegates to specific handlers
    /// - OCP: New workflows added via DI, no modification needed
    /// - LSP: All handlers implement interfaces
    /// - ISP: Separate interfaces for different concerns
    /// - DIP: Depends on abstractions (factories)
    /// </summary>
    public class CallbackHandler : ICallbackHandler
    {
        private readonly ILogger<CallbackHandler> _logger;
        private readonly IWorkflowHandlerFactory _workflowHandlerFactory;
        private readonly IActionHandlerFactory _actionHandlerFactory;

        public CallbackHandler(
        ILogger<CallbackHandler> logger,
        IWorkflowHandlerFactory workflowHandlerFactory,
        IActionHandlerFactory actionHandlerFactory)
        {
            _logger = logger;
            _workflowHandlerFactory = workflowHandlerFactory;
            _actionHandlerFactory = actionHandlerFactory;
        }


        /// <summary>
        /// Get available actions for a process
        /// Delegates to appropriate action handler based on workflow type
        /// </summary>
        public async Task<GetActionResponse> GetAvailableActionsAsync(string schemeCode)
        {
            _logger.LogInformation(
               "Getting available actions for scheme code {SchemeCode}",
               schemeCode);

            // Extract workflow type
            var workflowType = GetWorkflowType(schemeCode);

            // Get appropriate action handler
            var handler = _actionHandlerFactory.GetHandler(workflowType);

            // Delegate to handler
            return await handler.GetActionsAsync(schemeCode);
        }

        public async Task<ExecuteActionResponse> HandleActionExecutedAsync(ExecuteActionRequest callback)
        {

            var processInstance = ProcessInstanceHelper.Deserialize(callback.ProcessInstance);

            _logger.LogInformation(
                  "Action {ActionName} executed on process {ProcessId}",
                  callback.Name,
                  processInstance.ParentProcessId
                  );

            // Extract workflow type
            var workflowType = GetWorkflowType(processInstance.SchemeCode);

            // Get appropriate action handler
            var handler = _actionHandlerFactory.GetHandler(workflowType);

            // Delegate to handler
            return await handler.ExecuteActionAsync(callback);
        }

        public async Task HandleActivityChangedAsync(ProcessActivityChangedRequest callback)
        {
            _logger.LogInformation(
                 $"Activity changed for process {callback.ProcessId} ({callback.SchemeCode}): {callback.previousActivityName} → {callback.currentActivityName}",
                 callback.ProcessId,
                 callback.SchemeCode,
                 callback.previousActivityName,
                 callback.currentActivityName);


            // Extract workflow type
            var workflowType = GetWorkflowType(callback.SchemeCode);

            // Get appropriate handler
            var handler = _workflowHandlerFactory.GetHandler(workflowType);

            // Delegate to handler
            await handler.HandleActivityChangedAsync(callback);
        }

        /// <summary>
        /// Handle when process status changes (state transition)
        /// Delegates to appropriate handler based on workflow type
        /// </summary>
        public async Task HandleStatusChangedAsync(ProcessStatusChangedRequest callback)
        {
            _logger.LogInformation(
                   $"Status changed for process {callback.ProcessId} ({callback.SchemeCode}): {callback.OldStatus} → {callback.NewStatus}",
                   callback.ProcessId,
                   callback.SchemeCode,
                   callback.OldStatus,
                   callback.NewStatus);

            // Extract workflow type from SchemeCode
            var workflowType = GetWorkflowType(callback.SchemeCode);
            _logger.LogDebug("Detected workflow type: {WorkflowType}", workflowType);


            // Get appropriate handler (Strategy Pattern)
            var handler = _workflowHandlerFactory.GetHandler(workflowType);

            // Delegate to handler
            await handler.HandleStatusChangedAsync(callback);
        }


        // ========================================================================
        // Helper Methods
        // ========================================================================

        /// <summary>
        /// Extract workflow type from SchemeCode
        /// Example: "LeaveApproval_v1_1_0" → "LeaveApproval"
        /// </summary>
        private string GetWorkflowType(string schemeCode)
        {
            if (string.IsNullOrEmpty(schemeCode))
                return "Generic";

            // SchemeCode format: WorkflowType_v1_0_0 or WorkflowType_v1.0.0
            var parts = schemeCode.Split('_');
            if (parts.Length > 0)
            {
                return parts[0];
            }

            return schemeCode;
        }
    }
}
