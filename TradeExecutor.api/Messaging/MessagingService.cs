using System.Text.Json;
using Confluent.Kafka;
using TradeExecutor.api.Models;

namespace TradeExecutor.api.Messaging;

public interface IMessagingService
{
    Task PublishAsync(Trade trade);
}

public class MessagingService : IMessagingService
{
    private readonly ILogger<MessagingService> _logger;
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public MessagingService(
        IConfiguration configuration,
        ILogger<MessagingService> logger)
    {
        _logger = logger;
        var kafkaConfig = configuration.GetSection("Kafka").Get<KafkaConfig>();
        if (kafkaConfig == null || string.IsNullOrWhiteSpace(kafkaConfig.BootstrapServers))
        {
            throw new ArgumentException("Kafka configuration is missing or incomplete");
        }
        
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaConfig.BootstrapServers
        };
        
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _topic = kafkaConfig.Topic;
    }

    public async Task PublishAsync(Trade trade)
    {
        var message = JsonSerializer.Serialize(trade);

        try
        {
            var result = await _producer.ProduceAsync(_topic, new() { Key = trade.Id, Value = message });
            _logger.LogInformation("Published message: '{Message}' to topic '{Topic}' at offset {Offset}", message, result.Topic, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message: {Message}", message);
        }
        
    }
}