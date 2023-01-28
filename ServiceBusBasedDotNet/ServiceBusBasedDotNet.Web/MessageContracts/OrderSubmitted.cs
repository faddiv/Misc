using MassTransit;

namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderSubmitted : CorrelatedBy<Guid>
{
    public required Guid OrderId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string CustomerNumber { get; init; }

    Guid CorrelatedBy<Guid>.CorrelationId => OrderId;
}
