using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka.Configurations;
using Rcwowbagger.Kafka.Interfaces;
using Serilog;

namespace Rcwowbagger.Kafka;

public class KafkaPublisher<TKey, TValue> : IPublisher<TKey, TValue>
{
    private readonly ILogger _logger;
    private readonly KafkaConfiguration _config;
    private readonly ProducerConfig _producerConfig;
    private readonly IProducer<TKey, TValue> _producer;
    public KafkaPublisher(IConfiguration configuration)
    {
        _logger = Log.ForContext<KafkaPublisher<TKey, TValue>>();
        _config = configuration.GetSection("Kafka").Get<KafkaConfiguration>();

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers
        };

        _producer = new ProducerBuilder<TKey, TValue>(_producerConfig).Build();
    }


    public async Task PublishAsync(TValue message, string topic)
    {
        try
        {
            _ = await _producer.ProduceAsync(topic, new Message<TKey, TValue> { Value = message });
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.Warning(ex, "");
        }
    }
    public async Task PublishAsync(TKey key, TValue message, string topic)
    {
        try
        {
            _ = await _producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = message });
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.Warning(ex, "");
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }

}
