using MassTransit;
using ServiceBusBasedDotNet.Web.Warehouse.Contracts;

namespace ServiceBusBasedDotNet.Web.Warehouse.StateMachines;

public class AllocationStateMachine : MassTransitStateMachine<AllocationState>
{
    public State Allocated { get; set; }
    public State Released { get; set; }

    public Event<AllocationCreated> AllocationCreatedEvent { get; set; }
    public Event<ReleaseAllocationRequested> ReleaseAllocationRequestedEvent { get; set; }
    public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; set; }

    public AllocationStateMachine(ILogger<AllocationStateMachine> logger)
    {
        Event(() => AllocationCreatedEvent, x => x.CorrelateById(m => m.Message.AllocationId));
        Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
        {
            s.Delay = TimeSpan.FromSeconds(10);
            s.Received = cm => cm.CorrelateById(m => m.Message.AllocationId);
        });

        InstanceState(x => x.CurrentState);

        Initially(
            When(AllocationCreatedEvent)
                .Schedule(HoldExpiration, ctx =>
                {
                    logger.LogInformation("Shedule a AllocationHoldDurationExpired");
                    return new AllocationHoldDurationExpired
                    {
                        AllocationId = ctx.Message.AllocationId,
                        Quantity = ctx.Message.Quantity
                    };
                }, ctx => ctx.Message.HoldDuration)
                .TransitionTo(Allocated),
            When(ReleaseAllocationRequestedEvent)
                .Then(ctx =>
                {
                    logger.LogInformation("Out of order allocation release for {AllocationId}",
                        ctx.Message.AllocationId);
                })
                .TransitionTo(Released)
                );

        During(Allocated,
            When(HoldExpiration!.Received)
                .Then(ctx =>
                {
                    logger.LogInformation("Allocation expired");
                })
                .Finalize(),
            When(ReleaseAllocationRequestedEvent)
                .Unschedule(HoldExpiration)
                .Then(ctx =>
                {
                    logger.LogInformation("Allocation release requested");
                })
                .Finalize(),
            When(AllocationCreatedEvent)
                .Schedule(HoldExpiration, ctx =>
                {
                    logger.LogInformation("Already allocated but schedule HoldDurationExpired");
                    return new AllocationHoldDurationExpired
                    {
                        AllocationId = ctx.Message.AllocationId,
                        Quantity = ctx.Message.Quantity
                    };
                }, ctx => ctx.Message.HoldDuration)
            );

        During(Released,
            When(AllocationCreatedEvent)
                .Then(ctx =>
                {
                    logger.LogInformation("Allocation already released for {AllocationId}", ctx.Message.AllocationId);
                })
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
public class AllocationStateMachineDefinition : SagaDefinition<AllocationState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<AllocationState> sagaConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(3, 200));
        endpointConfigurator.UseInMemoryOutbox();
    }
}
