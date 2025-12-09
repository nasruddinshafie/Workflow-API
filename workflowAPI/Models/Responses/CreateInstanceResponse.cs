namespace workflowAPI.Models.Responses
{
    public class CreateInstanceResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

    }
}
