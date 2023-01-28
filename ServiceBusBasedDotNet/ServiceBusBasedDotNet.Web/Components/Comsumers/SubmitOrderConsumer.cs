using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrderBasic>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<SubmitOrderBasic> context)
    {
        _logger.LogInformation("Consume started");
        var submitOrder = context.Message;
        await Task.Delay(100);
        if (string.IsNullOrEmpty(submitOrder.CustomerNumber))
        {
            _logger.LogInformation("ERROR");
            throw new Exception("Big oof");
        }
        else if (submitOrder.CustomerNumber == "asdf")
        {
            _logger.LogInformation("Order Refused");
            if (context.ResponseAddress != null)
            {
                await context.RespondAsync(new OrderRejected
                {
                    Reason = $"Banned: {submitOrder.CustomerNumber}",
                    OrderId = submitOrder.OrderId,
                    Timestamp = submitOrder.Timestamp
                });
            }
        }
        else
        {
            _logger.LogInformation("Order Accepted");
            if (context.ResponseAddress != null)
            {
                await context.RespondAsync(new OrderAccepted
                {
                    CustomerNumber = submitOrder.CustomerNumber,
                    OrderId = submitOrder.OrderId,
                    Timestamp = submitOrder.Timestamp
                });
            }
            _logger.LogInformation("Response finished");
        }
    }
}
