using System.Text;
using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using ServiceDefaults;
using WebApi;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//this is to use with masstransit
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    
    busConfigurator.AddConsumer<MessageExampleConsumer>();

    busConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
    {
        var messaging = builder.Configuration.GetConnectionString("rabbitmq");
        rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
        rabbitMqBusFactoryConfigurator.Host(messaging);
                
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ConsumerWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.Map("/", () => Results.Redirect("/swagger"));

app.MapDefaultEndpoints();

app.MapPost("/sendMessage", (
    [FromServices] IConnection connection,
    [FromServices] ILogger<MessageExample> logger,
    MessageExample message,
    CancellationToken cancellationToken
) =>
{
    var correlationId = Guid.NewGuid().ToString();

    using var _ = logger.BeginScope(new Dictionary<string, object> {{"CorrelationId", correlationId}});

    if (message.GetType().GetProperty("CorrelationId") is not null)
        message.GetType().GetProperty("CorrelationId")!.SetValue(message, correlationId);

    using var channel = connection.CreateModel();
    channel.ExchangeDeclare("TestExchange", ExchangeType.Fanout);
    channel.QueueDeclare("TestQueue",
        true,
        false,
        false,
        null);

    channel.QueueBind("TestQueue", "TestExchange", routingKey: string.Empty);

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    var properties = channel.CreateBasicProperties();

    channel.BasicPublish("TestExchange",
        "",
        properties,
        body);

    logger.LogInformation("Message sent to {Exchange}/{Queue}", "TestExchange", "TestQueue");
    return Task.CompletedTask;
});

app.MapPost("/sendMessageWithMassTransit", async (
    [FromServices] IPublishEndpoint publishEndpoint,
    [FromServices] ILogger<MessageExample> logger,
    MessageExample message,
    CancellationToken cancellationToken
) =>
{
    var correlationId = Guid.NewGuid().ToString();

    using var _ = logger.BeginScope(new Dictionary<string, object> {{"CorrelationId", correlationId}});

    if (message.GetType().GetProperty("CorrelationId") is not null)
        message.GetType().GetProperty("CorrelationId")!.SetValue(message, correlationId);

    await publishEndpoint.Publish(message, cancellationToken);
    
    return Results.Accepted();
});

app.Run();