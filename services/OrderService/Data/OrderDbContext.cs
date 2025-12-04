using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> opts) : base(opts) { }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(i => i.POId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrder>()
                .HasIndex(p => p.PoNumber)
                .IsUnique();
        }
    }
}
