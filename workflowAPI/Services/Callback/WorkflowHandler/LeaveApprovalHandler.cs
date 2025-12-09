using workflowAPI.Models.Callbacks;
using workflowAPI.Models.Callbacks.Events;

namespace workflowAPI.Services.Callback.WorkflowHandler
{
    public class LeaveApprovalHandler : IWorkflowHandler
    {
        private readonly ILogger<LeaveApprovalHandler> _logger;
        private readonly INotificationService _notificationService;

        public string WorkflowType => "LeaveApproval";

        public LeaveApprovalHandler(
           ILogger<LeaveApprovalHandler> logger,
           INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }


        public bool CanHandle(string workflowType) => workflowType == WorkflowType;


        public async Task HandleActivityChangedAsync(ProcessActivityChangedRequest callback)
        {
            _logger.LogInformation(
               "Leave approval activity changed: {ProcessId}",
               callback.ProcessId);

            await Task.CompletedTask;
        }

        public async Task HandleStatusChangedAsync(ProcessStatusChangedRequest callback)
        {
            _logger.LogInformation(
                  "Handling Leave Approval status change: {ProcessId}, {OldStatus} → {callback.NewStatus}",
                  callback.ProcessId,
                  callback.OldStatus,
                  callback.NewStatus);

            var processInstance = ProcessInstanceHelper.Deserialize(callback.ProcessInstance)!;


            // Extract employee info
            var employeeId = ProcessInstanceHelper.GetParameter<string>(processInstance, "EmployeeId"); 
            var employeeName = ProcessInstanceHelper.GetParameter<string>(processInstance, "EmployeeName"); 


            // Send status change notification
            await _notificationService.SendLeaveStatusChangedNotificationAsync(
                callback.ProcessId.ToString(),
                employeeId,
                employeeName,
                callback.OldStatus,
                callback.NewStatus);


            // Handle specific states
            //switch (callback.CurrentState)
            //{
            //    case "Approved":
            //        await HandleApprovedAsync(callback, employeeId, employeeName);
            //        break;

            //    case "Rejected":
            //        await HandleRejectedAsync(callback, employeeId, employeeName);
            //        break;

            //    case "ManagerApproval":
            //        await HandleManagerApprovalNeededAsync(callback);
            //        break;

            //    case "HRApproval":
            //        await HandleHRApprovalNeededAsync(callback);
            //        break;

            //    case "Cancelled":
            //        await HandleCancelledAsync(callback);
            //        break;
            //}

            // Log event
            await _notificationService.LogWorkflowEventAsync(
                "LeaveStatusChanged",
                callback.ProcessId.ToString(),
                new Dictionary<string, object>
                {
                { "EmployeeId", employeeId },
                { "PreviousState", callback.OldStatus },
                { "CurrentState", callback.NewStatus }
                });

        }


        // Private methods for specific state handling

        private async Task HandleApprovedAsync(
            ProcessStatusChangedRequest callback,
            string employeeId,
            string employeeName)
        {
            _logger.LogInformation("Leave approved for {EmployeeName}", employeeName);

            await _notificationService.SendLeaveApprovedNotificationAsync(
                callback.ProcessId.ToString(),
                employeeId,
                employeeName);

            // TODO: Additional actions
            // - Update HR system
            // - Block calendar dates
            // - Update leave balance
        }

        private async Task HandleRejectedAsync(
            ProcessStatusChangedRequest callback,
            string employeeId,
            string employeeName)
        {
            var processInstance = ProcessInstanceHelper.Deserialize(callback.ProcessInstance)!;


            var comments = ProcessInstanceHelper.GetParameter<string>(processInstance, "Comments"); 
            _logger.LogInformation("Leave rejected for {EmployeeName}", employeeName);

            await _notificationService.SendLeaveRejectedNotificationAsync(
                callback.ProcessId.ToString(),
                employeeId,
                employeeName,
                comments);
        }

        private async Task HandleManagerApprovalNeededAsync(ProcessStatusChangedRequest callback)
        {
            _logger.LogInformation("Manager approval needed for {ProcessId}", callback.ProcessId);

            // TODO: Send notification to manager
            await Task.CompletedTask;
        }

        private async Task HandleHRApprovalNeededAsync(ProcessStatusChangedRequest callback)
        {
            _logger.LogInformation("HR approval needed for {ProcessId}", callback.ProcessId);

            // TODO: Send notification to HR
            await Task.CompletedTask;
        }

        private async Task HandleCancelledAsync(ProcessStatusChangedRequest callback)
        {
            _logger.LogInformation("Leave cancelled: {ProcessId}", callback.ProcessId);

            // TODO: Restore leave balance, unblock calendar
            await Task.CompletedTask;
        }
    }
}
