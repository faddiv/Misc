using MassTransit;
using MassTransit.Courier.Contracts;

namespace ServiceBusBasedDotNet.Web.Components.BatchConsumers;

public class RoutingSlipBatchEventConsumer : IConsumer<Batch<RoutingSlipCompleted>>
{
    private readonly ILogger<RoutingSlipBatchEventConsumer> _logger;

    public RoutingSlipBatchEventConsumer(ILogger<RoutingSlipBatchEventConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
    {
        _logger.LogInformation("Batch messages: {Count}", context.Message.Count());
        return Task.CompletedTask;
    }
}
public class RoutingSlipBatchEventConsumerDefinition : ConsumerDefinition<RoutingSlipBatchEventConsumer>
{
    public RoutingSlipBatchEventConsumerDefinition()
    {
    }
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<RoutingSlipBatchEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.PrefetchCount = 10;
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(10)
            .SetTimeLimit(TimeSpan.FromSeconds(10))
            .SetConcurrencyLimit(1));
    }
}
