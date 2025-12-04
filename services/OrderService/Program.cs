using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dto;
using OrderService.Models;
using OrderService.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddDbContext<OrderDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));
builder.Services.AddSingleton(sp =>
{
    var bootstrap = builder.Configuration.GetValue<string>("KAFKA__BOOTSTRAP");

    if (string.IsNullOrWhiteSpace(bootstrap))
    {
        throw new Exception("KAFKA__BOOTSTRAP is NOT set");
    }
    return new KafkaProducer(bootstrap);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<KafkaResultConsumerWorker>();
builder.Services.AddControllers();

var app = builder.Build();

// auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

// create PO
app.MapPost("/api/purchase-orders", async (CreatePoDto dto, OrderDbContext db, KafkaProducer producer) => {
    if (dto.Items == null || dto.Items.Count == 0) return Results.BadRequest("Items are required");
    foreach (var it in dto.Items)
        if (it.Quantity <= 0) return Results.BadRequest("Quantity must be more than 0");

    var po = new PurchaseOrder
    {
        PoNumber = dto.PoNumber ?? $"PO-{Guid.NewGuid():N}",
        Items = dto.Items.Select(i => new PurchaseOrderItem { ProductSKU = i.ProductSku, Quantity = i.Quantity }).ToList()
    };

    db.PurchaseOrders.Add(po);
    await db.SaveChangesAsync();

    // publish message
    var msg = new PoCreatedMessage(po.Id, po.PoNumber, po.CreatedAt,
    po.Items.Select(i => new PoItem(i.ProductSKU, i.Quantity)).ToList());

    await producer.ProduceAsync("po.created", po.Id.ToString(), msg);

    return Results.Created($"/api/purchase-orders/{po.Id}", po);
});

// get PO by ID
app.MapGet("/api/purchase_orders/{id:guid}", async (Guid id, OrderDbContext db) =>
{
    var po = await db.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
    return po is null ? Results.NotFound() : Results.Ok(po);
});

// List POs with optional status filter
app.MapGet("/api/purchase-orders", async (string? status, OrderDbContext db) =>
{
    var query = db.PurchaseOrders.Include(p => p.Items).AsQueryable();
    if (!string.IsNullOrEmpty(status) && Enum.TryParse<POStatus>(status, true, out var st))
        query = query.Where(p => p.Status == st);
    var list = await query.ToListAsync();
    return Results.Ok(list);
});

app.Run();
