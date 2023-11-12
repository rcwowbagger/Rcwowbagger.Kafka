using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka.Configurations;
using Rcwowbagger.Kafka.Interfaces;
using Serilog;

namespace Rcwowbagger.Kafka;

public class KafkaPublisher<T> : IPublisher<T>
{
    private readonly ILogger _logger;
    private readonly KafkaConfiguration _config;
    private readonly ProducerConfig _producerConfig;
    private readonly IProducer<Null,T> _producer;
    public KafkaPublisher(IConfiguration configuration)
    {
        _logger = Log.ForContext<KafkaPublisher<T>>();
        _config = configuration.GetSection("Kafka").Get<KafkaConfiguration>();

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, T>(_producerConfig).Build();
    }


    public async Task PublishAsync(T message)
    {
        try
        {
            _ = await _producer.ProduceAsync(_config.Topic, new Message<Null, T> { Value = message });
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
