namespace Rcwowbagger.Kafka.Interfaces;

public interface IPublisher<TKey, TValue> : IDisposable
{
    Task PublishAsync(TValue message, string topic);
    Task PublishAsync(TKey key, TValue message, string topic);
}
