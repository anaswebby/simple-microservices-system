using OrderService.Models;

namespace OrderService.Dto
{
    public record PoCreatedMessage(Guid PoId, string PoNumber, DateTime CreatedAt, List<PoItem> Items);
    public record PoItem(string ProductSKU, int Quantity);
}
