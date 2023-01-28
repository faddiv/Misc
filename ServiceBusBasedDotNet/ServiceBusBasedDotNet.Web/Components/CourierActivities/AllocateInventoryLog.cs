namespace ServiceBusBasedDotNet.Web.Components.CourierActivities
{
    public class AllocateInventoryLog
    {
        public required Guid AllocationId { get; init; }
        public required Guid OrderId { get; init; }
        public required int Quantity { get; init; }
    }
}
