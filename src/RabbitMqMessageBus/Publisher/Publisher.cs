using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitMqMessageBus.Publisher;

public interface IPublisher
{
    void Execute<T>(
        T message,
        PublisherParams publisherParams,
        CancellationToken cancellationToken = default) where T : class;
}

public class Publisher(IConnection connection, ILogger<Publisher> logger) : IPublisher
{
     public void Execute<T>(
         T message,
         PublisherParams publisherParams,
         CancellationToken cancellationToken = default) where T : class
     {
         ArgumentNullException.ThrowIfNull(message);
         ArgumentNullException.ThrowIfNull(publisherParams);
         
         if(string.IsNullOrEmpty(publisherParams.Exchange) && string.IsNullOrEmpty(publisherParams.Queue))
             throw new ArgumentException("You need to set an exchange or a queue name");

         var correlationId = publisherParams.CorrelationId ?? Guid.NewGuid().ToString();

         using var _ = logger.BeginScope(new Dictionary<string, object> {{"CorrelationId", correlationId}});
         
         if(message.GetType().GetProperty("CorrelationId") is not null)
             message.GetType().GetProperty("CorrelationId")!.SetValue(message, correlationId); 
         
         using var channel = connection.CreateModel();
         
         if(!string.IsNullOrEmpty(publisherParams.Exchange))
            channel.ExchangeDeclare(publisherParams.Exchange, 
                publisherParams.Type switch
                {
                    ExchangeTypeEnum.Direct => ExchangeType.Direct,
                    ExchangeTypeEnum.Fanout => ExchangeType.Fanout,
                    ExchangeTypeEnum.Headers => ExchangeType.Headers,
                    ExchangeTypeEnum.Topic => ExchangeType.Topic,
                    _ => throw new ArgumentOutOfRangeException(nameof(publisherParams.Type), "Invalid exchange Type")
                });

         if (!string.IsNullOrEmpty(publisherParams.Queue))
             channel.QueueDeclare(publisherParams.Queue,
                 true,
                 false,
                 false,
                 null);

         if (!string.IsNullOrEmpty(publisherParams.Exchange) && string.IsNullOrEmpty(publisherParams.Queue))
         {
             channel.QueueBind(publisherParams.Queue,
                 publisherParams.Exchange,
                 routingKey: string.Empty);
         }
         
         var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
         var properties = channel.CreateBasicProperties();

         properties.CorrelationId = correlationId;
         if (!string.IsNullOrWhiteSpace(publisherParams.ReplyTo))
             properties.ReplyTo = publisherParams.ReplyTo;
         
         channel.BasicPublish(publisherParams.Exchange,
             "",
             properties,
             body);

         logger.LogInformation("Message sent to {Exchange}/{Queue}", publisherParams.Exchange, publisherParams.Queue);
     }
}