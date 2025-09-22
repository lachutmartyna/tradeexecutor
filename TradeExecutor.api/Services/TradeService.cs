using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TradeExecutor.api.Data;
using TradeExecutor.api.Messaging;
using TradeExecutor.api.Models;

namespace TradeExecutor.api.Services;

public interface ITradeService
{
    Task<Trade> Add(TradeRequestModel tradeInput);
    Task<Trade> Update(TradeRequestModel request);
    Task<Trade?> Get(string tradeId);
    Task<List<Trade>> GetAll();
}

public class TradeService(
    TradeDbContext dbContext,
    IMessagingService messagingService,
    ILogger<TradeService> logger
) : ITradeService
{
    private Dictionary<string, Trade> _tradesCache = [];

    public async Task<Trade> Add(TradeRequestModel request)
    {
        var trade = new Trade
        {
            Id = Guid.NewGuid().ToString(),
            ExecutionTimestamp = DateTime.UtcNow,
            Status = TradeStatus.Initiated
        };
        MapTrade(trade, request);
        dbContext.Trades.Add(trade);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while saving trade with Id: '{tradeId}' to the database.", trade.Id);
            throw;
        }
        
        logger.LogInformation("Created trade with Id: {tradeId}", trade.Id);
        messagingService.PublishAsync(trade);
        _tradesCache[trade.Id] = trade;
        return trade;
    }

    public async Task<Trade> Update(TradeRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentException("Unable to update trade. Trade Id is missing");

        if (!_tradesCache.TryGetValue(request.Id, out var trade))
            throw new ArgumentException($"Unable to update trade. Trade with Id: '{request.Id}', does not exist");

        MapTrade(trade, request);
        dbContext.Trades.Update(trade);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated trade with Id: {tradeId}", trade.Id);
        messagingService.PublishAsync(trade);
        _tradesCache[trade.Id] = trade;
        return trade;
    }

    public async Task<Trade?> Get(string tradeId)
    {
        if (_tradesCache.IsNullOrEmpty()) await GetAll();

        return _tradesCache[tradeId];
    }

    public async Task<List<Trade>> GetAll()
    {
        if (_tradesCache.IsNullOrEmpty()) 
            _tradesCache = await dbContext.Trades.ToDictionaryAsync(trade => trade.Id);

        return _tradesCache.Values.ToList();
    }

    private static void MapTrade(Trade trade, TradeRequestModel request)
    {
        trade.Instrument = request.Instrument;
        trade.TradeType = Enum.Parse<TradeType>(request.TradeType, true);
        trade.Quantity = request.Quantity;
        trade.Price = request.Price;
        trade.Currency = request.Currency;
        trade.Counterparty = request.Counterparty;
        trade.Fees = request.Fees ?? 0m;
        trade.SettlementTimestamp = request.SettlementTimestamp ?? null;
        trade.CustomerId = request.CustomerId;
    }
}