using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqMessageBus.Publisher;
using Serilog.Context;

namespace RabbitMqMessageBus.Consumer;

public interface IConsumer
{
    Task ExecuteAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse?>> onMessage, ConsumerParams consumerParams,
        CancellationToken cancellationToken = default) where TRequest : class where TResponse : class;
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

        using var channel = connection.CreateModel();

        channel.QueueDeclare(consumerParams.Queue,
            true,
            false,
            false,
            null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            logger.LogInformation("Received message from queue {Queue} with id {MessageId}", consumerParams.Queue, ea.BasicProperties.MessageId);

            var correlationId = ea.BasicProperties.CorrelationId ?? Guid.NewGuid().ToString();
            var replyTo = ea.BasicProperties.ReplyTo ?? consumerParams.ReplyTo;

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var obj = JsonSerializer.Deserialize<TRequest>(message);

                if (obj?.GetType().GetProperty("CorrelationId") is not null)
                    obj.GetType().GetProperty("CorrelationId")!.SetValue(obj, correlationId);

                using var _ = LogContext.PushProperty("CorrelationId", correlationId);

                if (obj != null)
                    onMessage(obj);

                // if (!string.IsNullOrEmpty(replyTo))
//             {
//                 await Reply(replyTo, correlationId, result, cancellationToken);
//             }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                if (consumerParams.ExceptionTypeToDoNotRemoveFromQueue.Contains(e.GetType()))
                    logger.LogWarning("Expected error processing message {MessageId} {Exception}", ea.BasicProperties.MessageId, e.ToString());
                else
                    logger.LogError("Error processing message {MessageId} {Exception}", ea.BasicProperties.MessageId, e.ToString());

                // if (!string.IsNullOrEmpty(consumerParams.ErrorQueue))
                // {
                //     var senderParameters = new PublisherParams()
                //         .WithQueue(consumerParams.ErrorQueue)
                //         .WithCorrelationId(correlationId);
                //
                //     if(!string.IsNullOrWhiteSpace(replyTo))
                //         senderParameters.WithReplyTo(replyTo);
                //
                //     await publisher.ExecuteAsync(message.Body, senderParameters, cancellationToken: cancellationToken);
                // }

                var removeFromQueue = consumerParams.RemoveFromQueueOnException
                                      && (consumerParams.ExceptionTypeToDoNotRemoveFromQueue.Count == 0 ||
                                          !consumerParams.ExceptionTypeToDoNotRemoveFromQueue.Contains(e.GetType()));

                channel.BasicNack(ea.DeliveryTag, false, !removeFromQueue);
            }
        };

        channel.BasicConsume(consumerParams.Queue,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }


//
//     private async Task Reply<TResponse>(
//         string replyTo,
//         string correlationId,
//         TResponse? result,
//         CancellationToken cancellationToken)
//         where TResponse : class
//     {
//         var senderParameters = new PublisherParams()
//             .WithQueue(replyTo)
//             .WithCorrelationId(correlationId);
//
//         logger.LogInformation("Replying to queue: {QueueName}", replyTo);
//
//         if (result is not null)
//             await publisher.ExecuteAsync(result, senderParameters, cancellationToken);
//     }
}