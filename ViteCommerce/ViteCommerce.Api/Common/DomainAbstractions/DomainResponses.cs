using FluentValidation.Results;

namespace ViteCommerce.Api.Common.DomainAbstractions;

public static class DomainResponses
{
    public static DomainResponse<T> NotFound<T>() => NotFoundDomainResponse<T>.Instance;

    public static DomainResponse<T> ValidationFailed<T>(ValidationResult result)
        => DomainResponse<T>.ValidationFailed(ValidationError.Convert(result));

    public static DomainResponse<T> Ok<T>() => EmptyDomainResponse<T>.Instance;

    public static DomainResponse<T> OkOrNotFound<T>(T? value)
        => value is not null
            ? new OkDomainResponse<T>(value)
            : NotFound<T>();
}
