using MassTransit;
using MassTransit.Courier.Contracts;
using System.Text.Json;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class RoutingSlipEventConsumer : IConsumer<RoutingSlipCompleted>
{
    private readonly ILogger<RoutingSlipEventConsumer> _logger;

    public RoutingSlipEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
    {
        var msg = JsonSerializer.Serialize(context.Message);
        _logger.LogInformation("Routing slip completed. {Message}", msg);
        return Task.CompletedTask;
    }
}
