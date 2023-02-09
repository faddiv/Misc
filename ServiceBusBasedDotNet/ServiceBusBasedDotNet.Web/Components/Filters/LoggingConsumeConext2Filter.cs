using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class LoggingConsumeConext2Filter<TConsumer> : IFilter<ConsumerConsumeContext<TConsumer>>
        where TConsumer : class
{
    public LoggingConsumeConext2Filter()
    {
    }

    public async Task Send(ConsumerConsumeContext<TConsumer> context, IPipe<ConsumerConsumeContext<TConsumer>> next)
    {
        var _logger = context.GetServiceOrCreateInstance<ILogger<LoggingConsumeConext2Filter<TConsumer>>>();
        _logger.LogInformation("Before next: LoggingConsumeConext2Filter<TConsumer>");
        await next.Send(context);
        _logger.LogInformation("After next: LoggingConsumeConext2Filter<TConsumer>");
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("LoggingConsumeConext2Filter<TConsumer>");
    }
}
