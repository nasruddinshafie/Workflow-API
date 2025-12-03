namespace workflowAPI.Models.DTOs
{
    public class DocumentApprovalDto
    {
        public string DocumentId { get; set; } = string.Empty;
        public string SubmitterId { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public List<string> Approvers { get; set; } = new();
    }
}
