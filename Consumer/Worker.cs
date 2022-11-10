using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

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

        channel.BasicQos(0, 1, false);

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation(" [x] Received {0}", message);
            channel.BasicAck(ea.DeliveryTag, false);
        };
        channel.BasicConsume("test_queue",
            false,
            consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                $"Worker ativo em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await Task.Delay(5000, stoppingToken);
        }
    }
}