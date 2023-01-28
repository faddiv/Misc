namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class CustomerAccountClosed
{
    public required Guid CustomerId { get; init; }
    public required string CustomerNumber { get; init; }
}
