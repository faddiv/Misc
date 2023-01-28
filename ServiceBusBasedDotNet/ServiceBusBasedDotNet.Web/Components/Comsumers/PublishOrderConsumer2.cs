using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class PublishOrderConsumer2 : IConsumer<PublishOrder>
{
    private readonly ILogger<PublishOrderConsumer2> _logger;

    public PublishOrderConsumer2(ILogger<PublishOrderConsumer2> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<PublishOrder> context)
    {
        _logger.LogInformation("Consumer2 started");
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
        }
        else
        {
            _logger.LogInformation("Order Accepted 2");
            _logger.LogInformation("Response 2 finished");
        }
    }
}
