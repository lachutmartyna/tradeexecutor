using System.Reflection;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TradeExecutor.api.Messaging;
using TradeExecutor.api.Models;

namespace TradeExecutor.tests;

public class MessagingServiceTests
{
    private static readonly Dictionary<string, string> KafkaConfigDict = new()
    {
        {
            "Kafka:BootstrapServers", "localhost:9092"
        },
        {
            "Kafka:Topic", "trades"
        }
    };

    private readonly IConfigurationRoot _configuration;
    private readonly Mock<ILogger<MessagingService>> _mockLogger;
    private readonly Mock<IProducer<string, string>> _mockProducer;

    private readonly Trade _mockTrade = new()
    {
        Id = "2",
        Instrument = "AAPL",
        Quantity = 8,
        Price = 98.7m,
        Currency = "GBP",
        TradeType = TradeType.Buy
    };

    public MessagingServiceTests()
    {
        _mockLogger = new();
        _mockProducer = new();
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(KafkaConfigDict!).Build();
    }

    [Fact]
    public async Task PublishAsync_ValidTrade_PublishesMessage()
    {
        _mockProducer
            .Setup(p => p.ProduceAsync(
                "trades",
                It.Is<Message<string, string>>(m =>
                    m.Key == _mockTrade.Id &&
                    m.Value == JsonSerializer.Serialize(_mockTrade, JsonSerializerOptions.Default)),
                CancellationToken.None))
            .ReturnsAsync(new DeliveryResult<string, string>
            {
                Topic = "trades",
                Offset = new(1)
            });

        var service = new MessagingServiceForTest(_configuration, _mockLogger.Object, _mockProducer.Object);
        await service.PublishAsync(_mockTrade);
        _mockProducer.Verify(p => p.ProduceAsync(
            "trades",
            It.Is<Message<string, string>>(m =>
                m.Key == _mockTrade.Id &&
                m.Value == JsonSerializer.Serialize(_mockTrade, JsonSerializerOptions.Default)),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ProducerThrows_LogsError()
    {
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), default))
            .ThrowsAsync(new("Kafka error"));

        var service = new MessagingServiceForTest(_configuration, _mockLogger.Object, _mockProducer.Object);

        await service.PublishAsync(_mockTrade);

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to publish message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class MessagingServiceForTest : MessagingService
    {
        public MessagingServiceForTest(IConfiguration configuration, ILogger<MessagingService> logger,
            IProducer<string, string> producer)
            : base(configuration, logger)
        {
            typeof(MessagingService)
                .GetField("_producer",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(this, producer);
        }
    }
}