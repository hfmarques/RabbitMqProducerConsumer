using RabbitMQ.Client;

namespace RabbitMqMessageBus.Publisher;

public class PublisherParams
{
    public string Exchange { get; private set; } = "";
    public PublisherParams WithExchange(string exchange)
    {
        Exchange = exchange;
        return this;
    }
    
    public ExchangeTypeEnum Type { get; private set; } = ExchangeTypeEnum.Fanout;
    public PublisherParams WithExchangeType(ExchangeTypeEnum type)
    {
        Type = type;
        return this;
    }
    
    public string Queue { get; private set; } = default!;
    public PublisherParams WithQueue(string queue)
    {
        Queue = queue;
        return this;
    }
    
    public string? CorrelationId { get; private set; }
    public PublisherParams WithCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }

    public string? ReplyTo { get; set; }
    public PublisherParams WithReplyTo(string replyTo)
    {
        ReplyTo = replyTo;
        return this;
    }
}