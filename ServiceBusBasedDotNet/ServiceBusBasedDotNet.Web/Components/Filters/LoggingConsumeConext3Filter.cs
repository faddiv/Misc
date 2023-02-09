using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class LoggingConsumeConext3Filter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
{
    public LoggingConsumeConext3Filter()
    {
    }

    public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
    {
        var _logger = context.GetServiceOrCreateInstance<ILogger<LoggingConsumeConextFilter>>();
        _logger.LogInformation("Before next: LoggingConsumeConext3Filter<TConsumer, TMessage>");
        await next.Send(context);
        _logger.LogInformation("After next: LoggingConsumeConext3Filter<TConsumer, TMessage>");
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("LoggingConsumeConext3Filter<TConsumer, TMessage>");
    }
}
