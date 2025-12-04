using Confluent.Kafka;
using System.Text.Json;
using static InventoryService.Services.InventoryService;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Kafka
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _bootstrap;
        private readonly ProducerConfig _producerCfg;

        public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _bootstrap = config["KAFKA__BOOTSTRAP"] ?? "kafka:9092";
            _producerCfg = new ProducerConfig { BootstrapServers = _bootstrap };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
            return Task.CompletedTask;
        }

        private void Publish(string topic, string key, object payload)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrap
            };

            using var producer = new ProducerBuilder<string, string>(config).Build();

            var json = JsonSerializer.Serialize(payload);

            producer.Produce(topic, new Message<string, string>
            {
                Key = key,
                Value = json
            });

            producer.Flush(TimeSpan.FromSeconds(5));
        }

        private void ConsumeLoop(CancellationToken stoppingToken)
        {
            var conf = new ConsumerConfig
            {
                BootstrapServers = _bootstrap,
                GroupId = "inventory-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(conf).Build();
            consumer.Subscribe("po.created");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);

                    var msg = JsonSerializer.Deserialize<PoDto>(
                        cr.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (msg != null)
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var inventoryService =
                            scope.ServiceProvider.GetRequiredService<InventoryService.Services.InventoryService>();

                        var (ok, reason) = inventoryService
                            .TryReserveAsync(msg)
                            .GetAwaiter()
                            .GetResult();

                        if (ok)
                        {
                            var payload = new
                            {
                                poId = msg.PoId,
                                poNumber = msg.PoNumber,
                                status = "CONFIRMED",
                                message = "PO confirmed",
                                timestamp = DateTime.UtcNow
                            };

                            Publish("po.confirmed", msg.PoId.ToString(), payload);
                        }
                        else
                        {
                            var payload = new
                            {
                                poId = msg.PoId,
                                poNumber = msg.PoNumber,
                                status = "REJECTED",
                                message = reason,
                                timestamp = DateTime.UtcNow
                            };

                            Publish("po.rejected", msg.PoId.ToString(), payload);
                        }
                    }

                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
