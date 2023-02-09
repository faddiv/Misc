using MassTransit;
using MassTransit.Configuration;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class LoggingConsumeConext4Filter<TMessage> : IFilter<ConsumeContext<TMessage>>
        where TMessage : class
{
    public LoggingConsumeConext4Filter()
    {
    }
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("LoggingConsumeConext4Filter");
    }

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        // No scope here yet.
        //var _logger = context.GetServiceOrCreateInstance<ILogger<LoggingConsumeConextFilter>>();
        Console.WriteLine("Before next: LoggingConsumeConext4Filter<TMessage>");
        await next.Send(context);
        Console.WriteLine("After next: LoggingConsumeConext4Filter<TMessage>");
    }
}
