using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace ViteCommerce.Api.Common.DomainAbstractions;

[StructLayout(LayoutKind.Auto)]
public readonly struct DomainResponse<T> : IResponse<DomainResponse<T>>
{
    public ResponseState State { get; }
    public T? Value { get; }
    public Exception? Error { get; }

    public DomainResponse()
    {
        State = ResponseState.Empty;
        Value = default;
        Error = null;
    }

    public DomainResponse(T? value)
    {
        State = value is null ? ResponseState.Empty : ResponseState.Success;
        Value = value;
        Error = null;
    }

    public DomainResponse(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        State = ResponseState.Fail;
        Value = default;
        Error = error;
    }

    [Pure]
    public static DomainResponse<T> ToFail(Exception exception)
    {
        return new DomainResponse<T>(exception);
    }

    [Pure]
    public DomainResponse<R> OnSuccess<R>(Func<T, R> func)
    {
        return State switch
        {
            ResponseState.Empty => new DomainResponse<R>(default(R)),
            ResponseState.Success => new DomainResponse<R>(func(Value!)),
            ResponseState.Fail => new DomainResponse<R>(Error!),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    [Pure]
    public DomainResponse<T> OnFail(Func<Exception, T> func)
    {
        return State switch
        {
            ResponseState.Empty => this,
            ResponseState.Success => this,
            ResponseState.Fail => new DomainResponse<T>(func(Error!)),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    [Pure]
    public DomainResponse<T> OnFail<TException>(Func<TException, T> func)
        where TException : Exception
    {
        return State switch
        {
            ResponseState.Empty => this,
            ResponseState.Success => this,
            ResponseState.Fail when Error is TException exception => new DomainResponse<T>(func(exception)),
            _ => throw new ApplicationException($"Response<{typeof(T).Name}> is in illegal State: {State}"),
        };
    }

    [Pure]
    public DomainResponse<T> OnEmpty(Func<T> func)
    {
        return State switch
        {
            ResponseState.Empty => new DomainResponse<T>(func()),
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
    public static implicit operator DomainResponse<T>(T? value) => new(value);

    [Pure]
    public static implicit operator DomainResponse<T>(Exception error) => new(error);

    [Pure]
    public static explicit operator T(DomainResponse<T> response) => response.ToValue();
}
public enum ResponseState : byte
{
    Empty,
    Success,
    Fail
}
