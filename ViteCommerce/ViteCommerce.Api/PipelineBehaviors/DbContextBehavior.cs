using Database;
using MediatR;

namespace ViteCommerce.Api.PipelineBehaviors;

public class DbContextBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public DbContextBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TMessage request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await _unitOfWork.GetSessionAsync(cancellationToken);
        return await next();
    }
}
