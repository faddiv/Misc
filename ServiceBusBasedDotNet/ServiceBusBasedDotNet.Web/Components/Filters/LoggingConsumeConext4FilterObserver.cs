using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class LoggingConsumeConext4FilterObserver : IConsumerConfigurationObserver
{
    private readonly IConsumePipeConfigurator _consumePipeConfigurator;

    public LoggingConsumeConext4FilterObserver(IConsumePipeConfigurator consumePipeConfigurator)
    {
        _consumePipeConfigurator = consumePipeConfigurator;
    }
    public void ConsumerConfigured<TConsumer>(IConsumerConfigurator<TConsumer> configurator) where TConsumer : class
    {
    }

    public void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
        where TConsumer : class
        where TMessage : class
    {
        _consumePipeConfigurator.UseFilter(new LoggingConsumeConext4Filter<TMessage>());
    }
}
