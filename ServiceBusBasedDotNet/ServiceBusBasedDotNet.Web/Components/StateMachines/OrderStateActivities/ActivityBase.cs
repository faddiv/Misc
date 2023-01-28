using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines.OrderStateActivities;

public class ActivityBase<TSaga, TMessage> : IStateMachineActivity<TSaga, TMessage>
    where TSaga : class, ISaga
    where TMessage : class
{
    public virtual void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public virtual Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
    {
        return next.Execute(context);
    }

    public virtual Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public virtual void Probe(ProbeContext context)
    {
        var scope = context.CreateScope(GetType().Name);
    }
}
