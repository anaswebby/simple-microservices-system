using InventoryService.Data;
using InventoryService.Kafka;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddDbContext<InventoryDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));
builder.Services.AddScoped<InventoryService.Services.InventoryService>();
builder.Services.AddHostedService<KafkaConsumerWorker>();

var app = builder.Build();

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.Migrate();
}

app.Run();
