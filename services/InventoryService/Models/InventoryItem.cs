namespace InventoryService.Models
{
    public class InventoryItem
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string ProductSKU { get; set; } = default!;
        public string ProductName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProcessedEvent
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string EventId { get; set; } = default!; // POId converted to string
        public string Topic { get; set; } = default!;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
