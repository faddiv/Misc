using FluentValidation.Results;

namespace ViteCommerce.Api.Common.DomainAbstractions;

public static class DomainResponses
{
    public static DomainResponse<T> NotFound<T>() => new();

    public static DomainResponse<T> ValidationFailed<T>(ValidationResult result)
        => new ValidationFailedException(ValidationError.Convert(result));

    public static DomainResponse<T> OkOrEmpty<T>(T? value) => new(value);
}
