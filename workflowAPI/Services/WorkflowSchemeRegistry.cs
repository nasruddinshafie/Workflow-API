
using Microsoft.Extensions.Options;
using workflowAPI.Configuration;

namespace workflowAPI.Services
{
    public class WorkflowSchemeRegistry : IWorkflowSchemeRegistry
    {
        private readonly ILogger<WorkflowSchemeRegistry> _logger;
        private readonly Dictionary<string, SchemeConfig> _schemes;


        public WorkflowSchemeRegistry(
       IOptions<WorkflowConfiguration> config,
       ILogger<WorkflowSchemeRegistry> logger)
        {
            _logger = logger;
            _schemes = config.Value.Schemes;

            _logger.LogInformation(
                "Loaded {Count} workflow scheme configurations: {Types}",
                _schemes.Count,
                string.Join(", ", _schemes.Keys));
        }

        public string GetActiveScheme(string workflowType)
        {
            if (!_schemes.ContainsKey(workflowType))
            {
                _logger.LogError("Workflow type {WorkflowType} not found in registry", workflowType);
                throw new ArgumentException($"Unknown workflow type: {workflowType}");
            }

            var activeScheme = _schemes[workflowType].ActiveVersion;

            _logger.LogDebug(
                "Retrieved active scheme for {WorkflowType}: {SchemeCode}",
                workflowType,
                activeScheme);

            return activeScheme;
        }

        public List<string> GetAllWorkflowTypes()
        {
            return _schemes.Keys.ToList();
        }

        public List<string> GetAvailableVersions(string workflowType)
        {
            if (!_schemes.ContainsKey(workflowType))
                return new List<string>();

            return _schemes[workflowType].Versions.Keys.ToList();
        }

        public string? GetSchemeVersion(string workflowType, string version)
        {
            if (!_schemes.ContainsKey(workflowType))
                return null;

            var config = _schemes[workflowType];
            return config.Versions.GetValueOrDefault(version);
        }

        public bool IsValidWorkflowType(string workflowType)
        {
            return _schemes.ContainsKey(workflowType);
        }
    }
}
