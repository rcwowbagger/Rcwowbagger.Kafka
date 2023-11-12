namespace Rcwowbagger.Kafka.Interfaces;

public interface IConsumer<T>
{
    event Action<T> OnMessage;

    Task SubscribeAsync(CancellationToken cancellationToken);
}
