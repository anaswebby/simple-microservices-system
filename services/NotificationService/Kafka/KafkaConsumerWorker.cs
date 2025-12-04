using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;
using System.Text.Json;

namespace NotificationService.Kafka
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly string _bootstrap;

        public KafkaConsumerWorker(IServiceProvider sp, IConfiguration cfg)
        {
            _sp = sp;
            _bootstrap = cfg["KAFKA__BOOTSTRAP"] ?? "kafka:9092";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => Consume(stoppingToken), stoppingToken);
            return Task.CompletedTask;
        }

        private void Consume(CancellationToken stoppingToken)
        {
            var conf = new ConsumerConfig
            {
                BootstrapServers = _bootstrap,
                GroupId = "notification-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(conf).Build();
            consumer.Subscribe(new[] { "po.confirmed", "po.rejected" });

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var doc = JsonSerializer.Deserialize<JsonElement>(cr.Message.Value);
                    var poId = doc.GetProperty("poId").GetGuid();
                    var status = doc.GetProperty("status").GetString() ?? "UNKNOWN";
                    var message = doc.GetProperty("message").GetString() ?? string.Empty;

                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                    db.AuditLogs.Add(new AuditLog { PoId = poId, Status = status, Message = message, CreatedAt = DateTime.UtcNow });
                    db.SaveChanges();

                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException) { }
            finally { consumer.Close(); }
        }
    }
}
