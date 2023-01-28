using MassTransit;

namespace ServiceBusBasedDotNet.Web.Warehouse.Contracts;

public class ReleaseAllocationRequested : CorrelatedBy<Guid>
{
    public required Guid AllocationId { get; init; }
    public required string Reason { get; init; }
    public required int Quantity { get; init; }

    Guid CorrelatedBy<Guid>.CorrelationId => AllocationId;
}
