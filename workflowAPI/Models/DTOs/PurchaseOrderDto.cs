namespace workflowAPI.Models.DTOs
{
    public class PurchaseOrderDto
    {
        public string RequestorId { get; set; } = string.Empty;
        public string RequestorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public List<PurchaseItemDto> Items { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
