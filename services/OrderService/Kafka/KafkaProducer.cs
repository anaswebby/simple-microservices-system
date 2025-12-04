using Confluent.Kafka;
using System.Text.Json;

namespace OrderService.Kafka
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        public KafkaProducer(string bootstrapServers)
        {
            var cfg = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<string, string>(cfg).Build();
        }

        public async Task ProduceAsync(string topic, string key, object payload)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json });
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
