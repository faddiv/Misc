namespace ServiceBusBasedDotNet.Web.Components.CourierActivities
{
    public class AllocateInventoryArguments
    {
        public Guid OrderId { get; set; }
        public string ItemNumber { get; set; }
        public int Quantity { get; set; }
    }
}
