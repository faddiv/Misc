using MassTransit;

namespace ServiceBusBasedDotNet.Web.Warehouse.StateMachines
{
    public class AllocationState :
        SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public Guid? HoldDurationToken { get; set; }

        public int Version { get; set; }
    }
}
