using MassTransit;
using MassTransit.Configuration;
using ServiceBusBasedDotNet.Web.Components.CourierActivities;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class FulfillOrderConsumer : IConsumer<FulfillOrder>
{
    private readonly ILogger<FulfillOrderConsumer> _logger;

    public FulfillOrderConsumer(ILogger<FulfillOrderConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<FulfillOrder> context)
    {
        if (string.IsNullOrEmpty(context.Message.ItemNumber) ||
            context.Message.ItemNumber.Length < 5)
        {
            throw new ApplicationException("Invalid item number");
        }
        if (context.Message.ItemNumber.StartsWith("rnd"))
        {
            if(Random.Shared.Next(4) > 0)
            {
                throw new HttpRequestException("Http exception happens randomly.");
            }
        }
        _logger.LogInformation("Creating Activity courier");
        var id = Guid.NewGuid();
        var builder = new RoutingSlipBuilder(id);

        builder.AddActivity("AllocateInventory",
            new Uri("queue:allocate-inventory_execute"),
            new AllocateInventoryArguments
        {
            ItemNumber = context.Message.ItemNumber,
            Quantity = context.Message.Quantity
        });

        builder.AddActivity("PaymentActivity",
            new Uri("queue:payment_execute"),
            new PaymentArguments
        {
            CardNumber = context.Message.CardNumber,
            Amount = context.Message.Quantity * 10
        });
        builder.AddVariable("orderId", context.Message.OrderId);

        await builder.AddSubscription(new Uri("exchange:order-state"), //context.SourceAddress,
            MassTransit.Courier.Contracts.RoutingSlipEvents.Faulted |
            MassTransit.Courier.Contracts.RoutingSlipEvents.Supplemental,
            MassTransit.Courier.Contracts.RoutingSlipEventContents.None, x => x.Send(new OrderFulfillmentFaulted
            {
                OrderId = context.Message.OrderId,
                Timestamp = DateTime.Now
            }));

        await builder.AddSubscription(new Uri("exchange:order-state"), //context.SourceAddress,
            MassTransit.Courier.Contracts.RoutingSlipEvents.Completed |
            MassTransit.Courier.Contracts.RoutingSlipEvents.Supplemental,
            MassTransit.Courier.Contracts.RoutingSlipEventContents.None, x => x.Send(new OrderFulfillmentCompleted
            {
                OrderId = context.Message.OrderId,
                Timestamp = DateTime.Now
            }));
        var routingSlip = builder.Build();

        _logger.LogInformation("Executing Activity courier {Uri}", new Uri("queue:allocate-inventory_execute"));
        await context.Execute(routingSlip);
    }
}
public class FulfillOrderConsumerDefinition : ConsumerDefinition<FulfillOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<FulfillOrderConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r =>
        {
            r.Ignore<ApplicationException>();
            r.Interval(5, 1000);
        });

        //Faulted messages thrown away instead to put in error pipeline.
        //endpointConfigurator.DiscardFaultedMessages(); 
    }
}
