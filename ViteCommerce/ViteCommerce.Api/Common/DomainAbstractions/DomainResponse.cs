using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace ViteCommerce.Api.Common.DomainAbstractions;

[StructLayout(LayoutKind.Auto)]
public readonly struct DomainResponse<T> : IResponse<DomainResponse<T>>
{
    private readonly Exception? _error;

    public T? Value { get; }
    public Exception? Error => IsEmpty ? null : _error;
    public bool IsEmpty => _error is EmptyException;
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => _error is not null && _error is not EmptyException;

    [Pure]
    public DomainResponse()
    {
        Value = default;
        _error = EmptyException.Instance;
    }

    [Pure]
    public DomainResponse(in T? value)
    {
        Value = value;
        _error = value is null ? EmptyException.Instance : null;
    }

    [Pure]
    public DomainResponse(in Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Value = default;
        _error = error;
    }

    [Pure]
    public R Match<R>(
        in Func<T, R> onSuccess,
        in Func<Exception, R> onFail,
        in Func<R> onEmpty)
    {
        if (IsError)
        {
            return onFail(Error);
        }
        else if (Value is not null)
        {
            return onSuccess(Value);
        }
        return onEmpty();
    }

    [Pure]
    public T ToValue()
    {

        if (IsError)
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

    internal class EmptyException : Exception
    {
        private EmptyException()
        {

        }
        public static Exception Instance = new();
    }
}
