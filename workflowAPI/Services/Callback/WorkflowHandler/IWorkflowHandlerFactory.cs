namespace workflowAPI.Services.Callback.WorkflowHandler
{

    /// <summary>
    /// Factory for creating workflow handlers
    /// Open/Closed Principle: Open for extension, closed for modification
    /// New handlers can be added via DI without modifying this class
    /// </summary>
    public interface IWorkflowHandlerFactory
    {
        /// <summary>
        /// Get appropriate handler for workflow type
        /// </summary>
        IWorkflowHandler GetHandler(string workflowType);
    }
   

}
