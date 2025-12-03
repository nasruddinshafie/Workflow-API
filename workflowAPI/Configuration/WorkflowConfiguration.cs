namespace workflowAPI.Configuration
{
    public class WorkflowConfiguration
    {
        public Dictionary<string, SchemeConfig> Schemes { get; set; } = new();

    }

    public class SchemeConfig
    {
        public string ActiveVersion { get; set; } = string.Empty;
        public string? OldVersion { get; set; }
        public Dictionary<string, string> Versions { get; set; } = new();
        public string DefaultVersion { get; set; } = string.Empty;
    }
}
