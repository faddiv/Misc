using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace ViteCommerce.Api.Common.DomainAbstractions;

public readonly struct DomainResponseV1<T> : IResponse<DomainResponseV1<T>>
{
    public ResponseState State { get; }
    public T? Value { get; }
    public Exception? Error { get; }

    public DomainResponseV1()
    {
        State = ResponseState.Empty;
        Value = default;
        Error = null;
    }

    public DomainResponseV1(T? value)
    {
        State = value is null ? ResponseState.Empty : ResponseState.Success;
        Value = value;
        Error = null;
    }

    public DomainResponseV1(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        State = ResponseState.Fail;
        Value = default;
        Error = error;
    }

    public static DomainResponseV1<T> ToFail(Exception exception)
    {
        return new DomainResponseV1<T>(exception);
    }

    public DomainResponseV1<R> OnSuccess<R>(Func<T, R> func)
    {
        return State switch
        {
            ResponseState.Empty => new DomainResponseV1<R>(default(R)),
            ResponseState.Success => new DomainResponseV1<R>(func(Value!)),
            ResponseState.Fail => new DomainResponseV1<R>(Error!),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    public DomainResponseV1<T> OnFail(Func<Exception, T> func)
    {
        return State switch
        {
            ResponseState.Empty => this,
            ResponseState.Success => this,
            ResponseState.Fail => new DomainResponseV1<T>(func(Error!)),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    public DomainResponseV1<T> OnFail<TException>(Func<TException, T> func)
        where TException : Exception
    {
        return State switch
        {
            ResponseState.Empty => this,
            ResponseState.Success => this,
            ResponseState.Fail when Error is TException exception => new DomainResponseV1<T>(func(exception)),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    public DomainResponseV1<T> OnEmpty(Func<T> func)
    {
        return State switch
        {
            ResponseState.Empty => new DomainResponseV1<T>(func()),
            ResponseState.Success => this,
            ResponseState.Fail => this,
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

    [Pure]
    public static implicit operator DomainResponseV1<T>(T? value) => new(value);

    [Pure]
    public static implicit operator DomainResponseV1<T>(Exception error) => new(error);

    [Pure]
    public static explicit operator T(DomainResponseV1<T> response) => response.ToValue();
}
public enum ResponseState : byte
{
    Empty,
    Success,
    Fail
}
