namespace NotificationService.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PoId { get; set; }
        public string Status { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
