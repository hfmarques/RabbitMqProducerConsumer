using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi;

public class ConsumerWorker(
    ILogger<ConsumerWorker> logger,
    IConnection connection)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        using var channel = connection.CreateModel();

        channel.QueueDeclare("TestQueue",
            true,
            false,
            false,
            null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            logger.LogInformation("Received message from queue {Queue} with id {MessageId}", "TestQueue", ea.BasicProperties.MessageId);

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var obj = JsonSerializer.Deserialize<MessageExample>(message);

            logger.LogInformation("Message Received: {message}", obj.Message);
            
            channel.BasicAck(ea.DeliveryTag, false);
            return Task.CompletedTask;
        };

        channel.BasicConsume("TestQueue",
            autoAck: false,
            consumer: consumer);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                $"Worker ativo em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await Task.Delay(5000, cancellationToken);
        }
    }
}