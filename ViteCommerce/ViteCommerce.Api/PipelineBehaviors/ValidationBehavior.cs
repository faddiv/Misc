using FluentValidation;
using Mediator;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.PipelineBehaviors;

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IMessage
    where TResponse : IDomainResponse<TResponse>
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehavior(IValidator<TMessage>? validator = null)
    {
        _validator = validator;
    }
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (_validator is null)
            return await next(message, cancellationToken);

        var context = new ValidationContext<TMessage>(message);
        var result = await _validator.ValidateAsync(context, cancellationToken);
        if (result.IsValid)
            return await next(message, cancellationToken);
        
        TResponse domainResponse = TResponse.ValidationFailed(result);
        return domainResponse;
    }
}
