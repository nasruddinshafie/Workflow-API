namespace workflowAPI.Services.Callback.ActionHandler
{
    /// <summary>
    /// Factory for creating action handlers
    /// Open/Closed Principle: Open for extension, closed for modification
    /// New handlers can be added via DI without modifying this class
    /// </summary>
    public interface IActionHandlerFactory
    {
        /// <summary>
        /// Get appropriate action handler for workflow type
        /// </summary>
        IActionHandler GetHandler(string workflowType);
    }
}
