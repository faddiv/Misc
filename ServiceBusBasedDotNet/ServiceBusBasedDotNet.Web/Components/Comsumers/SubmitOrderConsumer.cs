using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
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
            if (context.Message.Notes.HasValue)
            {
                _logger.LogInformation("Big note: {Note}", await context.Message.Notes.Value);
            }
            await context.Publish(new FulfillOrder
            {
                OrderId = submitOrder.OrderId,
                CardNumber = submitOrder.CardNumber,
                Quantity = submitOrder.Quantity,
                ItemNumber = submitOrder.ItemNumber,
            });
        }
    }
}
