using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace ViteCommerce.Api.Common.DomainAbstractions;

[StructLayout(LayoutKind.Auto)]
public readonly struct DomainResponse<T> : IResponse<DomainResponse<T>>
{
    public T? Value { get; }
    public Exception? Error { get; }

    [Pure]
    public DomainResponse()
    {
        Value = default;
        Error = null;
    }

    [Pure]
    public DomainResponse(in T? value)
    {
        Value = value;
        Error = null;
    }

    [Pure]
    public DomainResponse(in Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Value = default;
        Error = error;
    }

    [Pure]
    public R Match<R>(
        in Func<T, R> onSuccess,
        in Func<Exception, R> onFail,
        in Func<R> onEmpty)
    {
        if (Error is not null)
        {
            return onFail(Error);
        }
        if (Value is not null)
        {
            return onSuccess(Value);
        }
        return onEmpty();
    }

    [Pure]
    public T ToValue()
    {

        if (Error is not null)
        {
            throw Error;
        }
        if (Value is not null)
        {
            return Value;
        }
        throw new NullReferenceException($"Response<{typeof(T).Name}> is empty. Can't cast to back.");
    }

    public static DomainResponse<T> ToFail(Exception exception)
    {
        return new(exception);
    }

    [Pure]
    public static implicit operator DomainResponse<T>(T? value) => new(value);

    [Pure]
    public static implicit operator DomainResponse<T>(Exception error) => new(error);

    [Pure]
    public static explicit operator T(DomainResponse<T> response) => response.ToValue();
}
