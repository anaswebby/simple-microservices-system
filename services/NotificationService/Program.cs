using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<NotificationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
