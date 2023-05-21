using FluentValidation;
using MediatR;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.PipelineBehaviors;

public class ValidationBehaviorMediatr<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IRequest<TResponse>
    where TResponse : IDomainResponse<TResponse>
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehaviorMediatr(IValidator<TMessage>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(TMessage request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator is null)
            return await next();

        var context = new ValidationContext<TMessage>(request);
        var result = await _validator.ValidateAsync(context, cancellationToken);
        if (result.IsValid)
            return await next();

        TResponse domainResponse = TResponse.ValidationFailed(result);
        return domainResponse;
    }
}


public class ValidationBehaviorMediatr2<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : notnull, IRequest<TResponse>
    where TResponse : IDomainResponse<TResponse>
{
    private readonly IValidator<TMessage>? _validator;

    public ValidationBehaviorMediatr2(IValidator<TMessage>? validator = null)
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

        TResponse domainResponse = TResponse.ValidationFailed(result);
        return domainResponse;
    }
}
