namespace workflowAPI.Models.Requests
{
    public class ExecuteCommandRequest
    {
        public string ProcessId { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string IdentityId { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
