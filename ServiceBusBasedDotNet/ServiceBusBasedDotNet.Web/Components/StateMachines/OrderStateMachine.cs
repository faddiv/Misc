using MassTransit;
using ServiceBusBasedDotNet.Web.Components.StateMachines.OrderStateActivities;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    private readonly ILogger<OrderStateMachine> _logger;

    public State Received { get; private set; }
    public State Completed { get; private set; }
    public State Cancelled { get; private set; }
    public State Faulted { get; private set; }

    public Event<SubmitOrder> SubmitOrder { get; private set; }
    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<CheckOrder> CheckOrder { get; private set; }
    public Event<CustomerAccountClosed> CustomerAccountClosed { get; private set; }
    public Event<OrderFulfillmentFaulted> OrderFulfillmentFaulted { get; private set; }
    public Event<OrderFulfillmentCompleted> OrderFulfillmentCompleted { get; private set; }
    public Event<Fault<FulfillOrder>> FaultFulfillOrder { get; private set; }

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        _logger = logger;
        Event(() => SubmitOrder);
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
        Event(() => CustomerAccountClosed,
            x => x.CorrelateBy((saga, ctx) => saga.CustomerNumber == ctx.Message.CustomerNumber));
        Event(() => OrderFulfillmentFaulted, x => x.CorrelateById(c => c.Message.OrderId));
        Event(() => OrderFulfillmentCompleted, x => x.CorrelateById(c => c.Message.OrderId));
        Event(() => FaultFulfillOrder, x => x.CorrelateById(c => c.Message.Message.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(SubmitOrder)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerNumber = ctx.Message.CustomerNumber;
                    ctx.Saga.CardNumber = ctx.Message.CardNumber;
                    ctx.Saga.ItemNumber = ctx.Message.ItemNumber;
                })
                .Then(Logger)
                .TransitionTo(Received)
        );

        During(Received,
            When(OrderFulfillmentCompleted)
                .Then(Logger)
                .TransitionTo(Completed),
            Ignore(SubmitOrder),
            When(OrderFulfillmentFaulted)
                .Then(c => { _logger.LogInformation("Order faulted happened"); })
                .TransitionTo(Faulted),
            When(FaultFulfillOrder)
                .Then(c => { _logger.LogInformation("FulfillOrder threw an exception"); })
                .TransitionTo(Faulted));

        DuringAny(
            When(CheckOrder)
                .Activity(x => x.OfType<CheckOrderActivity>()) //.Respond(() => new OrderStatus())
        );

        During(Completed,
            Ignore(OrderSubmitted),
            When(CustomerAccountClosed)
                .TransitionTo(Cancelled),
            When(OrderFulfillmentFaulted)
                .Then(c => { _logger.LogInformation("Order faulted happened"); })
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
