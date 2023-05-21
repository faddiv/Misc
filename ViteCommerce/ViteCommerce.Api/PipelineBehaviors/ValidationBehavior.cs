using FluentValidation;
using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.PipelineBehaviors;

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IRequest<TResponse>
    where TResponse : IDomainResponse<TResponse>
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehavior(IValidator<TMessage>? validator = null)
    {
        _validator = validator;
    }

    public Task<TResponse> Handle(TMessage request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return _validator is null
            ? next()
            : ValidateAndHandle(request, next, cancellationToken);
    }

    private async Task<TResponse> ValidateAndHandle(TMessage request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TMessage>(request);
        var result = await _validator!.ValidateAsync(context, cancellationToken);
        if (result.IsValid)
            return await next();

        var validationErrors = ValidationError.Convert(result);
        return TResponse.ValidationFailed(validationErrors);
    }
}
