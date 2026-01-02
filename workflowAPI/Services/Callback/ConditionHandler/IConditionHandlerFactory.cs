namespace workflowAPI.Services.Callback.ConditionHandler
{

    /// <summary>
    /// Factory for creating condition handlers
    /// Open/Closed Principle: Open for extension, closed for modification
    /// New handlers can be added via DI without modifying this class
    /// </summary>
    public interface IConditionHandlerFactory
    {
        /// <summary>
        /// Get appropriate condition handler for workflow type
        /// </summary>
        IConditionHandler GetHandler(string workflowType);
    }
}
