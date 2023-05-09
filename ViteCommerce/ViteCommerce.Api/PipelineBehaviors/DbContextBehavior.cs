using Database;
using Mediator;

namespace ViteCommerce.Api.PipelineBehaviors;

public class DbContextBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IMessage
{
    private readonly IUnitOfWork _unitOfWork;

    public DbContextBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        await _unitOfWork.GetSessionAsync();
        return await next(message, cancellationToken);
    }
}
