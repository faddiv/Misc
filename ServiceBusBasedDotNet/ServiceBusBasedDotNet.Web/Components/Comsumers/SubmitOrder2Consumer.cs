using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class SubmitOrder2Consumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrder2Consumer> _logger;

    public SubmitOrder2Consumer(ILogger<SubmitOrder2Consumer> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        _logger.LogInformation("OrderSubmittedConsumer started");
        var submitOrder = context.Message;
        //await Task.Delay(100);
        if (string.IsNullOrEmpty(submitOrder.CustomerNumber))
        {
            _logger.LogInformation("ERROR");
            throw new Exception("Big oof");
        }
        else if (submitOrder.CustomerNumber == "asdf")
        {
            _logger.LogInformation("Order Refused");
            await context.Publish(new OrderRejected
            {
                Reason = $"{submitOrder.CustomerNumber} cant submit order",
                OrderId = submitOrder.OrderId,
                Timestamp = submitOrder.Timestamp
            });
        }
        else
        {
            await context.Publish(new FulfillOrder
            {
                OrderId = submitOrder.OrderId,
                CardNumber = submitOrder.CardNumber,
                Quantity = submitOrder.Quantity,
                ItemNumber = submitOrder.ItemNumber,
            });
            await context.Publish(new OrderSubmitted
            {
                CustomerNumber = submitOrder.CustomerNumber,
                OrderId = submitOrder.OrderId,
                Timestamp = submitOrder.Timestamp
            });
        }
    }
}
