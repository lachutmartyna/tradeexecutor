namespace TradeExecutor.api.Models;

public record Trade
{
    public string Id { get; set; }
    public string CustomerId { get; set; }
    public string Instrument { get; set; }
    public TradeType TradeType { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime ExecutionTimestamp { get; set; }
    public DateTime? SettlementTimestamp { get; set; }
    public TradeStatus Status { get; set; }
    public string Currency { get; set; }
    public string Counterparty { get; set; }
    public decimal Fees { get; set; }
}

public enum TradeType
{
    Buy,
    Sell
}

public enum TradeStatus
{
    Initiated,
    Pending,
    Executed,
    Settled,
    Cancelled,
    Rejected
}