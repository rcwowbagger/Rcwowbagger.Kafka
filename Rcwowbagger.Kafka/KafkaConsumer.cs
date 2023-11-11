using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka.Configurations;
using Rcwowbagger.Kafka.Interfaces;
using Serilog;
using System.Collections.Concurrent;

namespace Rcwowbagger.Kafka;

public class KafkaConsumer : IConsumer
{
    public readonly ILogger _logger;
    private readonly KafkaConfiguration? _config;
    public event Action<string> OnMessage;
    private readonly BlockingCollection<string> _queue = new();


    public KafkaConsumer(IConfiguration configuration)
    {
        _logger = Log.ForContext<KafkaConsumer>();
        _config = configuration.GetSection("Kafka").Get<KafkaConfiguration>();

        _logger.Information("{@config}", _config);
    }

    private async Task BeginPublishAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryTake(out var item, 10_000, cancellationToken))
                {
                    OnMessage?.Invoke(item);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Warning(ex, "");
            }
        }
    }

    public async Task SubscribeAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () => BeginPublishAsync(cancellationToken));

        var config = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            ClientId = _config.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        try
        {
            _logger.Information("Beginning consume...");

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(_config.Topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(cancellationToken);
                    _queue.Add(result.Value);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.Warning(ex, "");
        }
    }
}