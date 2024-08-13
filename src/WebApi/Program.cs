using Microsoft.AspNetCore.Mvc;
using RabbitMqMessageBus.Publisher;
using ServiceDefaults;
using WebApi;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ConsumerWorker>();

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
        .WithExchange("TestExchange")
        .WithQueue("TestQueue")
        .WithCorrelationId(Guid.NewGuid().ToString());
    publisher.Execute(
        message,
        paramsSendInvoiceToCustomer,
        cancellationToken: cancellationToken); 
});

app.Run();

internal record MessageExample(string Message);