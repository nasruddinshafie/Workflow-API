namespace workflowAPI.Models.Responses
{
    public class CommandsResponse
    {
        public List<CommandResponse> Data { get; set; } = new();
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        // Helper property to maintain backward compatibility
        public List<CommandResponse> Commands => Data;
    }
}
