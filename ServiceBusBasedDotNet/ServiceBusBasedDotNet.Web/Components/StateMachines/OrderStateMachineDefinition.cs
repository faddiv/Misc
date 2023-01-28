using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class OrderStateMachineDefinition : SagaDefinition<OrderState>
{
    public OrderStateMachineDefinition()
    {
        var epName = KebabCaseEndpointNameFormatter.Instance.Saga<OrderState>();
        base.EndpointName = epName;
    }
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(15, 1000));
        endpointConfigurator.UseInMemoryOutbox();
    }
}
