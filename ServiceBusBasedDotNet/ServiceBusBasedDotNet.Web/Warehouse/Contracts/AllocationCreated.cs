namespace ServiceBusBasedDotNet.Web.Warehouse.Contracts;

public class AllocationCreated
{
    public Guid AllocationId { get; set; }
    public TimeSpan HoldDuration { get; set; }
    public int Quantity { get; set; }
}
