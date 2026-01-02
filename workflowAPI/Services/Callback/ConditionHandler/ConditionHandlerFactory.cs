using workflowAPI.Services.Callback.ActionHandler;

namespace workflowAPI.Services.Callback.ConditionHandler
{
    public class ConditionHandlerFactory : IConditionHandlerFactory
    {

        private readonly IEnumerable<IConditionHandler> _handlers;
        private readonly ILogger<ActionHandlerFactory> _logger;


        public ConditionHandlerFactory(IEnumerable<IConditionHandler> handlers, ILogger<ActionHandlerFactory> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }
        public IConditionHandler GetHandler(string workflowType)
        {
            _logger.LogDebug("Finding condition handler for workflow type: {WorkflowType}", workflowType);

            // Find specific handler
            var handler = _handlers.FirstOrDefault(h =>
                h.CanHandle(workflowType) &&
                h.WorkflowType != "Generic");

            if (handler != null)
            {
                _logger.LogDebug("Found specific condition handler: {HandlerType}", handler.GetType().Name);
                return handler;
            }

            // Fallback to generic handler
            var genericHandler = _handlers.First(h => h.WorkflowType == "Generic");
            _logger.LogDebug("Using generic action handler for unknown workflow type: {WorkflowType}", workflowType);

            return genericHandler;
        }
    }
}
