using MassTransit;

namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class CheckOrder : CorrelatedBy<Guid>
{
    public Guid OrderId { get; set; }

    Guid CorrelatedBy<Guid>.CorrelationId => OrderId;
}
