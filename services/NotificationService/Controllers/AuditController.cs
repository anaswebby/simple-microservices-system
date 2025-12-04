using Microsoft.AspNetCore.Mvc;
using NotificationService.Data;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly NotificationDbContext _db;
        public AuditController(NotificationDbContext db) => _db = db;

        [HttpGet("{poId:guid}")]
        public async Task<IActionResult> Get(Guid poId)
        {
            var logs = await _db.AuditLogs.Where(a => a.PoId == poId).ToListAsync();
            return Ok(logs);
        }
    }
}
