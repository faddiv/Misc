using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponses
{
    public static SelfContainedDomainResponse<T> NotFound<T>() => new SelfContainedDomainResponse<T>(DomainResponseStatus.NotFound);

    public static SelfContainedDomainResponse<T> Ok<T>() => new SelfContainedDomainResponse<T>(DomainResponseStatus.NoContent);

    public static SelfContainedDomainResponse<T> ValidationFailed<T>(ValidationResult validationResult)
        => SelfContainedDomainResponse<T>.ValidationFailed(validationResult);

    public static SelfContainedDomainResponse<T> Wrap<T>(T? value)
        => value is not null
            ? new SelfContainedDomainResponse<T>(value)
            : NotFound<T>();
}

public static class DomainResponses2
{
    public static DomainResponseBase<T> NotFound<T>() => NotFoundDomainResponse<T>.Instance;

    public static DomainResponseBase<T> Ok<T>() => OkDomainResponse<T>.Instance;

    public static DomainResponseBase<T> ValidationFailed<T>(ValidationResult validationResult)
        => DomainResponseBase<T>.ValidationFailed(validationResult);

    public static DomainResponseBase<T> Wrap<T>(T? value)
        => value is not null
            ? new DomainResponse<T>(value)
            : NotFound<T>();
}
