namespace ViteCommerce.Api.Common.DomainAbstractions;

public sealed class OkDomainResponse<T> : DomainResponse<T>
{
    public OkDomainResponse(T value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        Value = value;
    }

    public T Value { get; }
}
