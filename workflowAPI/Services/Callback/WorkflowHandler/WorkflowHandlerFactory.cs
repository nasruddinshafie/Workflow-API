namespace workflowAPI.Services.Callback.WorkflowHandler
{
    public class WorkflowHandlerFactory : IWorkflowHandlerFactory
    {
        private readonly IEnumerable<IWorkflowHandler> _handlers;
        private readonly ILogger<WorkflowHandlerFactory> _logger;

        public WorkflowHandlerFactory(
            IEnumerable<IWorkflowHandler> handlers,
            ILogger<WorkflowHandlerFactory> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public IWorkflowHandler GetHandler(string workflowType)
        {
            _logger.LogDebug("Finding handler for workflow type: {WorkflowType}", workflowType);

            // Find specific handler
            var handler = _handlers.FirstOrDefault(h =>
                h.CanHandle(workflowType) &&
                h.WorkflowType != "Generic");

            if (handler != null)
            {
                _logger.LogDebug("Found specific handler: {HandlerType}", handler.GetType().Name);
                return handler;
            }

            // Fallback to generic handler
            var genericHandler = _handlers.First(h => h.WorkflowType == "Generic");
            _logger.LogDebug("Using generic handler for unknown workflow type: {WorkflowType}", workflowType);

            return genericHandler;
        }
    }
}
