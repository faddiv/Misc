using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;

namespace ViteCommerce.Api.Common.ValidationResults;

public interface IDomainResponse<T>
    where T : IDomainResponse<T>
{
    static abstract T ValidationFailed(ValidationResult result);
}

public sealed class SelfContainedDomainResponse<T> : IDomainResponse<SelfContainedDomainResponse<T>>
{
    public SelfContainedDomainResponse(T value)
    {
        Value = value;
        StatusCode = DomainResponseStatus.Ok;
    }

    public SelfContainedDomainResponse(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
        StatusCode = DomainResponseStatus.Failed;
    }

    public SelfContainedDomainResponse(DomainResponseStatus statusCode)
    {
        StatusCode = statusCode;
    }

    public DomainResponseStatus StatusCode { get; }

    public T? Value { get; }

    public IReadOnlyList<ValidationError>? Errors { get; }

    public static SelfContainedDomainResponse<T> ValidationFailed(ValidationResult result)
    {
        return new SelfContainedDomainResponse<T>(ValidationError.Convert(result));
    }
}


public class DomainResponseBase<T> : IDomainResponse<DomainResponseBase<T>>
{
    public DomainResponseBase()
    {
    }

    public static DomainResponseBase<T> ValidationFailed(ValidationResult result)
    {
        return new ValidationFailedDomainResponse<T>(ValidationError.Convert(result));
    }
}
public sealed class DomainResponse<T> : DomainResponseBase<T>
{
    public DomainResponse(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
