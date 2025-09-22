using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TradeExecutor.api.Controllers;
using TradeExecutor.api.Models;
using TradeExecutor.api.Services;

namespace TradeExecutor.tests;

public class TradeControllerTests
{
    private readonly Mock<ITradeService> _mockService;
    private readonly Mock<ILogger<TradeController>> _mockLogger;
    private readonly TradeController _controller;

    private readonly TradeRequestModel _mockTradeRequest = new()
    {
        Id = "1",
        Instrument = "AAPL",
        Quantity = 10,
        Price = 175.5m,
        Currency = "EUR",
        TradeType = "Sell"
    };

    private readonly Trade _mockTrade = new()
    {
        Id = "2",
        Instrument = "AAPL",
        Quantity = 8,
        Price = 98.7m,
        Currency = "GBP",
        TradeType = TradeType.Buy
    };

    public TradeControllerTests()
    {
        _mockLogger = new();
        _mockService = new();
        _controller = new(_mockLogger.Object, _mockService.Object);
    }

    [Fact]
    public async Task Add_ValidTradeRequest_ReturnsOk()
    {
        _mockService.Setup(s => s.Add(It.IsAny<TradeRequestModel>())).ReturnsAsync(_mockTrade);

        var result = await _controller.Add(_mockTradeRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(_mockTrade.Id, okResult.Value);
    }

    [Fact]
    public async Task Update_ValidTradeRequest_ReturnsOk()
    {
        const string tradeId = "2";
        var trade = new Trade
        {
            Id = tradeId,
            Instrument = "AAPL",
            Quantity = 10,
            Price = 175.5m,
            Counterparty = "broker",
            TradeType = TradeType.Sell,
            Currency = "PLN"
        };

        _mockService.Setup(s => s.Update(It.IsAny<TradeRequestModel>())).ReturnsAsync(trade);

        var result = await _controller.Update(tradeId, _mockTradeRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tradeId, okResult.Value);
    }

    [Fact]
    public async Task Update_EmptyId_ReturnsBadRequest()
    {
        var mockService = new Mock<ITradeService>();
        var mockLogger = new Mock<ILogger<TradeController>>();
        var controller = new TradeController(mockLogger.Object, mockService.Object);

        var tradeRequest = new TradeRequestModel();

        var result = await controller.Update("", tradeRequest);

        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task Get_ExistingTrade_ReturnsOkWithTrade()
    {
        _mockService.Setup(s => s.Get(_mockTrade.Id)).ReturnsAsync(_mockTrade);

        var result = await _controller.Get(_mockTrade.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTrade = Assert.IsType<Trade>(okResult.Value);
        Assert.Equal("2", returnedTrade.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTrades()
    {
        var trades = new List<Trade>
        {
            new()
            {
                Id = "4",
                Instrument = "AAPL",
                Quantity = 10,
                Price = 175.5m,
                TradeType = TradeType.Sell,
                Currency = "EUR"
            },
            new()
            {
                Id = "5",
                Instrument = "MSFT",
                Quantity = 5,
                Price = 300.0m,
                TradeType = TradeType.Buy,
                Currency = "EUR"
            }
        };
        _mockService.Setup(s => s.GetAll()).ReturnsAsync(trades);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTrades = Assert.IsAssignableFrom<IEnumerable<Trade>>(okResult.Value);
        Assert.Equal(2, returnedTrades.Count());
    }
}