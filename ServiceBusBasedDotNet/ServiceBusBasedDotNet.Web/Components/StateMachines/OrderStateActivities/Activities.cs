using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines.OrderStateActivities;

public class Activities
{
}


public class CheckOrderActivity : ActivityBase<OrderState, CheckOrder>
{
    private readonly ILogger<CheckOrderActivity> _logger;

    public CheckOrderActivity(ILogger<CheckOrderActivity> logger)
    {
        _logger = logger;
    }
    public override async Task Execute(
        BehaviorContext<OrderState, CheckOrder> context,
        IBehavior<OrderState, CheckOrder> next)
    {
        _logger.LogInformation("Order info requested on {CorrelationId}", context.Saga.CorrelationId);
        await context.RespondAsync(new OrderStatus 
        {
            OrderId = context.Saga.CorrelationId,
            CustomerNumber = context.Saga.CustomerNumber,
            State = context.Saga.CurrentState
        });
        _logger.LogInformation("Order info provided on {CorrelationId}", context.Saga.CorrelationId);
        await next.Execute(context);
    }
}
