namespace workflowAPI.Models.Requests
{
    public class WriteLogRequest
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? Token { get; set; }
    }
}
