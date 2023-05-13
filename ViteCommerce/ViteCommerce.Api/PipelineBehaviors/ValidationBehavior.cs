using FluentValidation;
using Mediator;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.PipelineBehaviors;

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IMessage
    where TResponse : IInvalidOr<TResponse>
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehavior(IValidator<TMessage>? validators)
    {
        _validator = validators;
    }
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var context = new ValidationContext<TMessage>(message);
        if(_validator is not null)
        {
            var result = await _validator.ValidateAsync(context).ConfigureAwait(false);
            if(!result.IsValid)
            {
                return TResponse.Create(result);
            }
        }
        return await next(message, cancellationToken);
    }
}
