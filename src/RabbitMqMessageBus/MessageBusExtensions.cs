using Microsoft.Extensions.DependencyInjection;
using RabbitMqMessageBus.Consumer;
using RabbitMqMessageBus.Publisher;

namespace RabbitMqMessageBus;

public static class MessageBusExtensions
{
    public static void AddServicesFromMessageBus(this IServiceCollection services)
    {
        services.AddTransient<IPublisher, Publisher.Publisher>();
        services.AddTransient<IConsumer, Consumer.Consumer>();
    }
}