using MassTransit;

namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderStatus : CorrelatedBy<Guid>
{
    public Guid OrderId { get; set; }
    public string State { get; set; }
    public string CustomerNumber { get; set; }

    Guid CorrelatedBy<Guid>.CorrelationId => OrderId;
}
