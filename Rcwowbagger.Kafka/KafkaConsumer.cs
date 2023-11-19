using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka.Configurations;
using Rcwowbagger.Kafka.Interfaces;
using Serilog;
using System.Collections.Concurrent;

namespace Rcwowbagger.Kafka;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
{
    public readonly ILogger _logger;
    private readonly KafkaConfiguration? _config;
    public event Action<TKey, TValue> OnMessage;
    private readonly BlockingCollection<(TKey key, TValue value)> _queue = new();


    public KafkaConsumer(IConfiguration configuration)
    {
        _logger = Log.ForContext<KafkaConsumer<TKey, TValue>>();
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
                    OnMessage?.Invoke(item.key, item.value);
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
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = _config.EnableAutoCommit ?? true
        };

        try
        {
            _logger.Information("Beginning consume...");
            using (var consumer = new ConsumerBuilder<TKey, TValue>(config).Build())
            {

                var topics = GetTopics().Where(x => x.StartsWith(_config.TopicPrefix)).ToList();
                consumer.Subscribe(topics);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(cancellationToken);
                    _queue.Add((result.Key, result.Value));
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.Warning(ex, "");
        }
    }

    public List<string> GetTopics()
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = _config.BootstrapServers
        };
        using (var adminClient = new AdminClientBuilder(config).Build())
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topicNames = metadata.Topics.Select(a => a.Topic).ToList();
            return topicNames;
        }
    }
}