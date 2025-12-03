namespace workflowAPI.Models.Requests
{
    public class CreateInstanceRequest
    {
        public string SchemeCode { get; set; } = string.Empty;
        public string IdentityId { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
