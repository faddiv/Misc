namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderFulfillmentFaulted
{
    public Guid OrderId { get; set; }

    public DateTime Timestamp { get; set; }
}
