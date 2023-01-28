using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class OrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string CustomerNumber { get; set; }
    public int Version { get; set; }
}
