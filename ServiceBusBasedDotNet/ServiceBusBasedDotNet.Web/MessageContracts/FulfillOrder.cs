namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class FulfillOrder
{
    public Guid OrderId { get; set; }
    public string CardNumber { get; set; }
    public string ItemNumber { get; set; }
    public int Quantity { get; set; }
}
