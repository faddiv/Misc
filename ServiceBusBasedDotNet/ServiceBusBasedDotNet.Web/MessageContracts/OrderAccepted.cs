namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderAccepted
{
    public required Guid OrderId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string CustomerNumber { get; init; }
}
