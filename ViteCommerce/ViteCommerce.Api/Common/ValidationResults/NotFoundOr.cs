using Amazon.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace ViteCommerce.Api.Common.ValidationResults;

public class NotFoundOr<TResponse> : INotFoundOr<NotFoundOr<TResponse>>
{
    private static readonly NotFoundOr<TResponse> _instance = new();
    private NotFoundOr() { }
    public NotFoundOr(TResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    public TResponse? Value { get; }

    [MemberNotNull(nameof(Value))]
    public bool HasValue => !ReferenceEquals(this, _instance);

    public static NotFoundOr<TResponse> NotFoundResponse()
    {
        return _instance;
    }

    public static implicit operator NotFoundOr<TResponse>(TResponse? t) => t is not null ? new(t) : _instance;
}
