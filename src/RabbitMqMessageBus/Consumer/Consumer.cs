using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMqMessageBus.Publisher;

namespace RabbitMqMessageBus.Consumer;

public interface IConsumer
{
    Task ExecuteAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse?>> onMessage, ConsumerParams consumerParams, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class;
}

public class Consumer(IConnection connection, ILogger<Consumer> logger, IPublisher publisher) : IConsumer
{
     public Task ExecuteAsync<TRequest, TResponse>(
         Func<TRequest, Task<TResponse?>> onMessage,
         ConsumerParams consumerParams,
         CancellationToken cancellationToken = default)
         where TRequest : class where TResponse : class
     {
         ArgumentNullException.ThrowIfNull(consumerParams);
         ArgumentException.ThrowIfNullOrEmpty(consumerParams.Queue);

         return Task.CompletedTask;
     }
}
