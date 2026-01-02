namespace workflowAPI.Models.Callbacks.Requests
{
    public class ProcessLogsRequest
    {
        public List<ProcessLog> ProcessLogs { get; set; } = new List<ProcessLog>();
    }
}
