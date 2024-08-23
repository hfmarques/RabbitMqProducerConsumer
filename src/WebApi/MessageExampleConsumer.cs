using MassTransit;

namespace WebApi;

public class MessageExampleConsumer(
    ILogger<MessageExampleConsumer> logger)
    : IConsumer<MessageExample>
{
    public Task Consume(ConsumeContext<MessageExample> context)
    {
        logger.LogInformation("Received message from queue {Queue} with id {MessageId}", "TestQueue", context.MessageId);

        var obj = context.Message;

        logger.LogInformation("Message Received: {message}", obj.Message);
        
        return Task.CompletedTask;
    }
}