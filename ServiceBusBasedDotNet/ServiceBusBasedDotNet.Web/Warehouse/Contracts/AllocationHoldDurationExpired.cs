namespace ServiceBusBasedDotNet.Web.Warehouse.Contracts
{
    public class AllocationHoldDurationExpired
    {
        public Guid AllocationId { get; set; }
        public int Quantity { get; set; }
    }
}
