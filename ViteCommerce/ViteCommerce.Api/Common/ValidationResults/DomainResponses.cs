using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponses
{
    public static IDomainResponse NotFound => NotFoundDomainResponse.Instance;

    public static IDomainResponse Ok => OkDomainResponse.Instance;

    public static IDomainResponse ValidationFailed(ValidationResult validationResult)
        => new ValidationFailedDomainResponse(validationResult);

    public static IDomainResponse Wrap(object? value)
        => value is not null
            ? new DomainResponseWrapper(value)
            : NotFound;
}
