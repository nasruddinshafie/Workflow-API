namespace workflowAPI.Models.Responses
{
    public class CreateInstanceResponse
    {
        public string ProcessId { get; set; } = string.Empty;
        public string SchemeCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
    }
}
