using Confluent.Kafka;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dto;
using OrderService.Models;

public class KafkaResultConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    public KafkaResultConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var conf = new ConsumerConfig
        {
            BootstrapServers = _config["KAFKA__BOOTSTRAP"],
            GroupId = "order-result-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(conf).Build();

        consumer.Subscribe(new[] { "po.confirmed", "po.rejected" });

        while (!stoppingToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(stoppingToken);

            var result = JsonSerializer.Deserialize<PoResultDto>(
                cr.Message.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null) continue;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var po = db.PurchaseOrders
                       .FirstOrDefault(x => x.Id == result.PoId);

            if (po == null) continue;

            po.Status = result.Status == "CONFIRMED"
                ? POStatus.CONFIRMED
                : POStatus.REJECTED;

            db.SaveChanges();
        }
    }
}
