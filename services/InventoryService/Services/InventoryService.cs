using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryService.Services
{
    public class InventoryService
    {
        private readonly InventoryDbContext _db;
        public InventoryService(InventoryDbContext db) => _db = db;

        public async Task<(bool ok, string? reason)> TryReserveAsync(PoDto po)
        {
            if (await _db.ProcessedEvents.AnyAsync(e => e.EventId == po.PoId.ToString() && e.Topic == "po.created")) return (true, null);

            var skus = po.Items.Select(i => i.ProductSku).Distinct().ToList();
            var items = await _db.InventoryItems.Where(i => skus.Contains(i.ProductSKU)).ToListAsync();

            foreach(var item in po.Items)
            {
                var inv = items.FirstOrDefault(x => x.ProductSKU == item.ProductSku);
                if (inv == null) return (false, $"SKU {item.ProductSku} not found");
                if (inv.AvailableQuantity < item.Quantity) return (false, $"insufficient stock for {item.ProductSku}");
            }

            // transactional deduct
            using var tx = await _db.Database.BeginTransactionAsync();
            foreach(var item in po.Items)
            {
                var inv = items.First(i => i.ProductSKU == item.ProductSku);
                inv.AvailableQuantity -= item.Quantity;
                inv.UpdatedAt = DateTime.UtcNow;
                _db.InventoryItems.Update(inv);
            }

            // record processed event for idempotency
            _db.ProcessedEvents.Add(new ProcessedEvent { EventId = po.PoId.ToString(), Topic = "po.created" });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null);
        }

        public record PoItemDto(string ProductSku, int Quantity);
        public record PoDto(System.Guid PoId, string PoNumber, System.DateTime CreatedAt, List<PoItemDto> Items);
    }
}
