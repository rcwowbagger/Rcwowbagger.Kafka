namespace Rcwowbagger.Kafka.Interfaces;

public interface IPublisher<T> : IDisposable
{
    Task PublishAsync(T message);
}
