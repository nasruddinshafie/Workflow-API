namespace workflowAPI.Services.Callback.ActionHandler
{
    public class ActionHandlerFactory : IActionHandlerFactory
    {
        private readonly IEnumerable<IActionHandler> _handlers;
        private readonly ILogger<ActionHandlerFactory> _logger;

        public ActionHandlerFactory(
            IEnumerable<IActionHandler> handlers,
            ILogger<ActionHandlerFactory> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public IActionHandler GetHandler(string workflowType)
        {
            _logger.LogDebug("Finding action handler for workflow type: {WorkflowType}", workflowType);

            // Find specific handler
            var handler = _handlers.FirstOrDefault(h =>
                h.CanHandle(workflowType) &&
                h.WorkflowType != "Generic");

            if (handler != null)
            {
                _logger.LogDebug("Found specific action handler: {HandlerType}", handler.GetType().Name);
                return handler;
            }

            // Fallback to generic handler
            var genericHandler = _handlers.First(h => h.WorkflowType == "Generic");
            _logger.LogDebug("Using generic action handler for unknown workflow type: {WorkflowType}", workflowType);

            return genericHandler;
        }
    }
}
