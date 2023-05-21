using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponses
{
    public static DomainResponseBase<T> NotFound<T>() => NotFoundDomainResponse<T>.Instance;

    public static DomainResponseBase<T> Ok<T>() => EmptyDomainResponse<T>.Instance;

    public static DomainResponseBase<T> ValidationFailed<T>(ValidationResult validationResult)
        => DomainResponseBase<T>.ValidationFailed(validationResult);

    public static DomainResponseBase<T> Wrap<T>(T? value)
        => value is not null
            ? new OkDomainResponse<T>(value)
            : NotFound<T>();
}
