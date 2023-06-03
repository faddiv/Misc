using ViteCommerce.Api.Common.DomainAbstractions;

namespace Benchmarks.ResultShape;


public readonly struct DomainResponseV2<T> : IResponse<DomainResponseV2<T>>
{
    public ResponseState State { get; }
    public T? Value { get; }
    public Exception? Error { get; }

    public DomainResponseV2()
    {
        State = ResponseState.Empty;
        Value = default;
        Error = null;
    }

    public DomainResponseV2(T? value)
    {
        State = value is null ? ResponseState.Empty : ResponseState.Success;
        Value = value;
        Error = null;
    }

    public DomainResponseV2(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        State = ResponseState.Fail;
        Value = default;
        Error = error;
    }

    public R Match<R>(
        Func<T, R> onSuccess,
        Func<Exception, R> onFail,
        Func<R> onEmpty)
    {
        return State switch
        {
            ResponseState.Success => onSuccess(Value),
            ResponseState.Fail => onFail(Error),
            ResponseState.Empty => onEmpty(),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    public T ToValue()
    {
        if (State is ResponseState.Success)
            return Value!;
        if (State is ResponseState.Empty)
            throw new NullReferenceException($"Response<{typeof(T).Name}> is empty. Can't cast to back.");
        if (State is ResponseState.Fail)
            throw Error!;
        throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}");
    }

    public static DomainResponseV2<T> ToFail(Exception exception)
    {
        return new(exception);
    }

    public static implicit operator DomainResponseV2<T>(T? value) => new(value);

    public static implicit operator DomainResponseV2<T>(Exception error) => new(error);

    public static explicit operator T(DomainResponseV2<T> response) => response.ToValue();
}
