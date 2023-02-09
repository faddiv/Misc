using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class LoggingConsumeConextFilter : IFilter<ConsumeContext>
{
    public LoggingConsumeConextFilter()
    {
    }
    public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
    {
        // No scope here yet.
        //var _logger = context.GetServiceOrCreateInstance<ILogger<LoggingConsumeConextFilter>>();
        Console.WriteLine("Before next: LoggingConsumeConextFilter");
        await next.Send(context);
        Console.WriteLine("After next: LoggingConsumeConextFilter");
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("LoggingConsumeConextFilter");
    }
}
