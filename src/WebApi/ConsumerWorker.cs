using RabbitMqMessageBus.Consumer;

namespace WebApi;

public class ConsumerWorker(
    ILogger<ConsumerWorker> logger,
    IConsumer consumer)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        
        var @params = new ConsumerParams()
            .WithQueue("TestQueue");

        await consumer.ExecuteAsync(
            async (MessageExample message) =>
            {
                logger.LogInformation("Message Received: {message}",message.Message);
                return Task.CompletedTask;
            }, 
            @params,
            cancellationToken);
    
        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                $"Worker ativo em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await Task.Delay(5000, cancellationToken);
        }
    }
}