namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class OrderFulfillmentCompleted
{
    public Guid OrderId { get; set; }

    public DateTime Timestamp { get; set; }
}
