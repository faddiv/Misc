using ViteCommerce.Api.Common.DomainAbstractions;

namespace Benchmarks.ResultShape;

//[StructLayout(LayoutKind.Auto)]
public readonly struct DomainResponseV3<T> : IResponse<DomainResponseV3<T>>
{
    public T? Value { get; }
    public Exception? Error { get; }

    //[Pure]
    public DomainResponseV3()
    {
        Value = default;
        Error = null;
    }

    //[Pure]
    public DomainResponseV3(T? value)
    {
        Value = value;
        Error = null;
    }

    //[Pure]
    public DomainResponseV3(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Value = default;
        Error = error;
    }

    //[Pure]
    public R Match<R>(
        Func<T, R> onSuccess,
        Func<Exception, R> onFail,
        Func<R> onEmpty)
    {
        if(Error is not null)
        {
            return onFail(Error);
        }
        if(Value is not null)
        {
            return onSuccess(Value);
        }
        return onEmpty();
    }

    //[Pure]
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

    public static DomainResponseV3<T> ToFail(Exception exception)
    {
        return new(exception);
    }

    //[Pure]
    public static implicit operator DomainResponseV3<T>(T? value) => new(value);

    //[Pure]
    public static implicit operator DomainResponseV3<T>(Exception error) => new(error);

    //[Pure]
    public static explicit operator T(DomainResponseV3<T> response) => response.ToValue();
}
