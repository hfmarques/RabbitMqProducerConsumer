using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Producer;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration.GetConnectionString("rabbitmq") ?? string.Empty),
                DispatchConsumersAsync = false,
                ConsumerDispatchConcurrency = 1,
                UseBackgroundThreadsForIO = false
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare("test_queue",
                true,
                false,
                false,
                null);

            var message = JsonSerializer.Serialize(new {id = Guid.NewGuid(), timestamp = DateTime.Now});
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("",
                "test_queue",
                null,
                body);
            _logger.LogInformation(" [x] Sent {0}", message);

            await Task.Delay(1000, stoppingToken);
        }
    }
}