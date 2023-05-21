
using FluentValidation.Results;
using ViteCommerce.Api.Common.ValidationResults;

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
public sealed class OkDomainResponse<T> : DomainResponseBase<T>
{
    public OkDomainResponse(T value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        Value = value;
    }

    public T Value { get; }
}
