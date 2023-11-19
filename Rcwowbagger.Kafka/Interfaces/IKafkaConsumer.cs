namespace Rcwowbagger.Kafka.Interfaces;

public interface IKafkaConsumer<TKey, TValue>
{
    event Action<TKey, TValue> OnMessage;

    Task SubscribeAsync(CancellationToken cancellationToken);
}
