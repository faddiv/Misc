using FluentValidation;
using Mediator;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.PipelineBehaviors;

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IMessage
    where TResponse : IDomainResponse
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehavior(IValidator<TMessage>? validators = null)
    {
        _validator = validators;
        if (_validator is null)
            return;

        var type = typeof(TResponse);
        if (type != typeof(IDomainResponse))
        {
            throw new ArgumentException($"The response type for {typeof(TMessage).Name} " +
                $"should be {nameof(IDomainResponse)} but found {typeof(TResponse).Name}.");
        }
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

        return (TResponse)DomainResponses.ValidationFailed(result);
    }
}
