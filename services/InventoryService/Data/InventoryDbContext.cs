using Microsoft.EntityFrameworkCore;
using InventoryService.Models;

namespace InventoryService.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> opts) : base(opts) { }
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<ProcessedEvent> ProcessedEvents { get; set; } = null!;
    }
}
