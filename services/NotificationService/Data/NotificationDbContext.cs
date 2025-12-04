using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> opts) : base(opts) { }
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    }
}
