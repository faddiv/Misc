using MassTransit;

namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class SubmitOrder : CorrelatedBy<Guid>
{
    public required Guid OrderId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string CustomerNumber { get; init; }
    public string CardNumber { get; set; }
    public int Quantity { get; set; }
    public string ItemNumber { get; set; }
    public MessageData<string> Notes { get; set; }

    Guid CorrelatedBy<Guid>.CorrelationId => OrderId;
}
