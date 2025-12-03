namespace workflowAPI.Models.Responses
{
    public class WorkflowInstanceResponse
    {
        public string ProcessId { get; set; } = string.Empty;
        public string SchemeCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string ActivityName { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
