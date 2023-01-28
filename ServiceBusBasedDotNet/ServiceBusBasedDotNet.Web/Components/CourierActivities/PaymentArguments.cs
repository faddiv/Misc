namespace ServiceBusBasedDotNet.Web.Components.CourierActivities;

public class PaymentArguments
{
    public Guid OrderId { get; set; }
    public int Amount { get; set; }
    public string CardNumber { get; set; }
}
