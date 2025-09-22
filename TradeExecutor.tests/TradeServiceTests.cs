using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TradeExecutor.api.Data;
using TradeExecutor.api.Messaging;
using TradeExecutor.api.Models;
using TradeExecutor.api.Services;

namespace TradeExecutor.tests;

public class TradeServiceTests
{
    private readonly TradeDbContext _dbContext;
    private readonly Mock<IMessagingService> _mockMessagingService;
    private readonly TradeService _tradeService;

    public TradeServiceTests()
    {
        var options = new DbContextOptionsBuilder<TradeDbContext>()
            .UseInMemoryDatabase("TradesDb_Test")
            .Options;

        _dbContext = new(options);
        _mockMessagingService = new();
        Mock<ILogger<TradeService>> mockLogger = new();
        _tradeService = new(_dbContext, _mockMessagingService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Add_ShouldAddTradeAndPublishMessage()
    {
        var tradeRequest = new TradeRequestModel
        {
            CustomerId = "user01",
            Instrument = "ABC",
            TradeType = "Buy",
            Quantity = 15,
            Price = 100m,
            Currency = "USD",
            Counterparty = "broker"
        };

        var trade = new Trade
        {
            Id = "1",
            CustomerId = "user01",
            Instrument = "ABC",
            TradeType = TradeType.Buy,
            Quantity = 15,
            Price = 100m,
            Currency = "USD",
            Counterparty = "broker"
        };

        _dbContext.Trades.Add(trade);

        var result = await _tradeService.Add(tradeRequest);

        Assert.NotNull(result);
        Assert.Equal("ABC", result.Instrument);
        _mockMessagingService.Verify(m => m.PublishAsync(It.IsAny<Trade>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldUpdateTradeAndPublishMessage()
    {
        var trade = new Trade
        {
            Id = "2",
            CustomerId = "user02",
            Instrument = "XYZ",
            TradeType = TradeType.Sell,
            Quantity = 5,
            Price = 50m,
            Currency = "EUR",
            Counterparty = "dealer"
        };
        _dbContext.Trades.Add(trade);
        await _dbContext.SaveChangesAsync();

        var tradeRequest = new TradeRequestModel
        {
            Id = "2",
            CustomerId = "user02",
            Instrument = "XYZ",
            TradeType = "Buy",
            Quantity = 10,
            Price = 55m,
            Currency = "EUR",
            Counterparty = "broker"
        };

        await _tradeService.GetAll();

        var result = await _tradeService.Update(tradeRequest);

        Assert.NotNull(result);
        Assert.Equal("2", result.Id);
        Assert.Equal(TradeType.Buy, result.TradeType);
        Assert.Equal(10, result.Quantity);
        _mockMessagingService.Verify(m => m.PublishAsync(It.IsAny<Trade>()), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldReturnTrade()
    {
        var trade = new Trade
        {
            Id = "3",
            CustomerId = "user03",
            Instrument = "DEF",
            TradeType = TradeType.Buy,
            Quantity = 20,
            Price = 200m,
            Currency = "GBP",
            Counterparty = "agent"
        };
        _dbContext.Trades.Add(trade);
        await _dbContext.SaveChangesAsync();

        await _tradeService.GetAll();

        var result = await _tradeService.Get("3");

        Assert.NotNull(result);
        Assert.Equal("3", result.Id);
        Assert.Equal("DEF", result.Instrument);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllTrades()
    {
        var trades = new List<Trade>
        {
            new()
            {
                Id = "4",
                Instrument = "AAA",
                TradeType = TradeType.Buy,
                Quantity = 1,
                Price = 10m,
                Currency = "USD",
                Counterparty = "cp1",
                CustomerId = "user_01"
            },
            new()
            {
                Id = "5",
                Instrument = "BBB",
                TradeType = TradeType.Sell,
                Quantity = 2,
                Price = 20m,
                Currency = "EUR",
                Counterparty = "cp2",
                CustomerId = "user_02"
            }
        };
        _dbContext.Trades.AddRange(trades);
        await _dbContext.SaveChangesAsync();

        var result = await _tradeService.GetAll();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == "4");
        Assert.Contains(result, t => t.Id == "5");
    }
}