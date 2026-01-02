namespace workflowAPI.Services
{
    public interface INotificationService
    {
        // Leave Approval notifications
        Task SendLeaveStatusChangedNotificationAsync(
            string processId,
            string employeeId,
            string employeeName,
            string previousState,
            string currentState);

        Task SendLeaveApprovedNotificationAsync(
            string processId,
            string employeeId,
            string employeeName);

        Task SendLeaveRejectedNotificationAsync(
            string processId,
            string employeeId,
            string employeeName,
            string reason);

        // Purchase Order notifications
        Task SendPurchaseOrderStatusChangedAsync(
            string processId,
            string requestorId,
            string requestorName,
            string previousState,
            string currentState,
            decimal amount,
            string vendor);

        Task SendPurchaseOrderApprovedAsync(
            string processId,
            string requestorId,
            string requestorName,
            decimal amount);

        // Document Approval notifications
        Task SendDocumentStatusChangedAsync(
            string processId,
            string submitterId,
            string submitterName,
            string documentType,
            string previousState,
            string currentState);

        Task SendDocumentApprovedAsync(
            string processId,
            string submitterId,
            string documentType);

        // Employee Onboarding notifications
        Task SendOnboardingStatusChangedAsync(
            string processId,
            string employeeId,
            string employeeName,
            string previousState,
            string currentState);

        Task SendOnboardingCompletedAsync(
            string processId,
            string employeeId,
            string employeeName);

        // Generic notifications
        Task SendGenericWorkflowNotificationAsync(
            string workflowType,
            string processId,
            string state,
            Dictionary<string, object> parameters);

        // Logging
        Task LogWorkflowEventAsync(
            string eventType,
            string processId,
            Dictionary<string, object> data);
    }
}
//please do optimization!!!!!