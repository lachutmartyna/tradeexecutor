namespace TradeExecutor.api.Models;

public record TradeRequestModel
{
    public string? Id { get; set; }
    public string CustomerId { get; set; }
    public string Instrument { get; set; }
    public string TradeType { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime? SettlementTimestamp { get; set; }
    public string Currency { get; set; }
    public string Counterparty { get; set; }
    public decimal? Fees { get; set; }
}