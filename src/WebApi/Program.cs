using Microsoft.AspNetCore.Mvc;
using RabbitMqMessageBus.Publisher;
using ServiceDefaults;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.Map("/", () => Results.Redirect("/swagger"));

app.MapDefaultEndpoints();

app.MapPost("/sendMessage", async (
    [FromServices] IPublisher publisher,
    MessageExample message,
    CancellationToken cancellationToken
) =>
{
    var paramsSendInvoiceToCustomer = new PublisherParams()
        .WithExchange("ExchangeTeste")
        .WithQueue("QueueTeste")
        .WithCorrelationId(Guid.NewGuid().ToString());
    publisher.Execute(
        message,
        paramsSendInvoiceToCustomer,
        cancellationToken: cancellationToken); 
});

// app.MapGet("/getMessage", async (
//     [FromServices] IConsumer consumer,
//     CancellationToken cancellationToken
// ) =>
// {
//     var @params = new ConsumerParams()
//         .WithMaxNumberOfMessages(1)
//         .WithWaitTimeSeconds(10)
//         .WithQueue("http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/test");
//
//     var receivedMessage = "";
//
//     await consumer.ExecuteAsync(
//         async (MessageExample message) =>
//         {
//             receivedMessage = message.Message;
//             return Task.CompletedTask;
//         }, 
//         @params,
//         cancellationToken);
//     
//     return Results.Ok(receivedMessage);
// });

app.Run();

internal record MessageExample(string Message);