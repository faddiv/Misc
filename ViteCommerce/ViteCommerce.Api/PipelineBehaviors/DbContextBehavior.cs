using Database;
using FluentValidation;
using Mediator;
using OneOf;
using ViteCommerce.Api.Common;

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

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IMessage
    where TResponse : IOneOf
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var context = new ValidationContext<TMessage>(message);
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context).ConfigureAwait(false);
            if(!result.IsValid)
            {
                return (dynamic)result;
            }
        }
        return await next(message, cancellationToken);
    }
}
