namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderRejected
{
    public required Guid OrderId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Reason { get; init; }
}
