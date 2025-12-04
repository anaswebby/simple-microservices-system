namespace OrderService.Models
{
    public enum POStatus { PENDING, CONFIRMED, REJECTED }

    public class PurchaseOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PoNumber { get; set; } = default!;
        public POStatus Status { get; set; } = POStatus.PENDING;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<PurchaseOrderItem> Items { get; set; } = new();
    }

    public class PurchaseOrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid POId { get; set; }
        public string ProductSKU { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
