namespace workflowAPI.Models.Callbacks.Requests
{
    public class ProcessLog
    {
        public Guid ProcessId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
