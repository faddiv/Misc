using MassTransit;
using ServiceBusBasedDotNet.Web.Components.StateMachines.OrderStateActivities;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    private readonly ILogger<OrderStateMachine> _logger;

    public State Received { get; private set; }
    public State Submitted { get; private set; }
    public State Cancelled { get; private set; }
    public State Faulted { get; private set; }

    public Event<SubmitOrder> SubmitOrder2 { get; private set; }
    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<CheckOrder> CheckOrder { get; private set; }
    public Event<CustomerAccountClosed> CustomerAccountClosed { get; private set; }
    public Event<OrderFulfillmentFaulted> OrderFulfillmentFaulted { get; set; }

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        _logger = logger;
        Event(() => SubmitOrder2);
        Event(() => OrderSubmitted);
        Event(() => CheckOrder, cfg =>
        {
            cfg.OnMissingInstance(m =>
            {
                return m.ExecuteAsync(async ctx =>
                {
                    await ctx.RespondAsync(new OrderStatus
                    {
                        OrderId = ctx.Message.OrderId,
                        CustomerNumber = "",
                        State = "Not exists"
                    });
                });
            });
        });
        Event(() => CustomerAccountClosed, x => x.CorrelateBy((saga, ctx) => saga.CustomerNumber == ctx.Message.CustomerNumber));
        Event(() => OrderFulfillmentFaulted, x => x.CorrelateById(c => c.Message.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(SubmitOrder2)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerNumber = ctx.Message.CustomerNumber;
                })
                .Then(Logger)
                .TransitionTo(Received)
            );

        During(Received,
            When(OrderSubmitted)
                .Then(Logger)
                .TransitionTo(Submitted),
            Ignore(SubmitOrder2),
            When(OrderFulfillmentFaulted)
                .Then(c =>
                {
                    _logger.LogInformation("Order faulted happened.");
                })
                .TransitionTo(Faulted));

        DuringAny(
            When(CheckOrder)
            .Activity(x => x.OfType<CheckOrderActivity>())//.Respond(() => new OrderStatus())
            );

        During(Submitted,
            Ignore(OrderSubmitted),
            When(CustomerAccountClosed)
                .TransitionTo(Cancelled),
            When(OrderFulfillmentFaulted)
                .Then(c =>
                {
                    _logger.LogInformation("Order faulted happened.");
                })
                .TransitionTo(Faulted));
    }

    public void Logger<TMessage>(BehaviorContext<OrderState, TMessage> context)
        where TMessage : class
    {
        _logger.LogInformation("Behavior triggered for {Type}:{CorrelationId}:{CustomerNumber}",
            typeof(TMessage).Name,
            context.CorrelationId,
            context.Saga.CustomerNumber);
    }
}
