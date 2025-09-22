namespace TradeExecutor.api.Messaging;

public record KafkaConfig
{
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
}