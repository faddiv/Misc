using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace Benchmarks.ResultShape;

[StructLayout(LayoutKind.Auto)]
public readonly struct DomainResponseV4<T> : IResponse<DomainResponseV4<T>>
{
    public T? Value { get; }
    public Exception? Error { get; }

    [Pure]
    public DomainResponseV4()
    {
        Value = default;
        Error = null;
    }

    [Pure]
    public DomainResponseV4(in T? value)
    {
        Value = value;
        Error = null;
    }

    [Pure]
    public DomainResponseV4(in Exception error)
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

    public static DomainResponseV4<T> ToFail(Exception exception)
    {
        return new(exception);
    }

    [Pure]
    public static implicit operator DomainResponseV4<T>(T? value) => new(value);

    [Pure]
    public static implicit operator DomainResponseV4<T>(Exception error) => new(error);

    [Pure]
    public static explicit operator T(DomainResponseV4<T> response) => response.ToValue();
}
