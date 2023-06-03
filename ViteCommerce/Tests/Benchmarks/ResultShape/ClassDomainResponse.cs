using ViteCommerce.Api.Common.DomainAbstractions;

namespace Benchmarks.ResultShape;


public abstract class ClassDomainResponse<T> : IResponse<ClassDomainResponse<T>>
{
    protected ClassDomainResponse()
    {
    }

    public static ClassDomainResponse<T> ToFail(Exception exception)
    {
        return new ValidationFailedDomainResponse<T>(exception);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFail, Func<TResult> onEmpty)
    {
        return this switch
        {
            OkDomainResponse<T> ok => onSuccess(ok.Value),
            ValidationFailedDomainResponse<T> fail => onFail(fail.Error),
            NotFoundDomainResponse<T> => onEmpty(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public sealed class ValidationFailedDomainResponse<T> : ClassDomainResponse<T>
{
    public ValidationFailedDomainResponse(Exception error)
    {
        Error = error;
    }

    public Exception Error { get; }

}

public sealed class OkDomainResponse<T> : ClassDomainResponse<T>
{
    public OkDomainResponse(T value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        Value = value;
    }

    public T Value { get; }
}

public sealed class NotFoundDomainResponse<T> : ClassDomainResponse<T>
{
    public static readonly NotFoundDomainResponse<T> Instance = new();
    private NotFoundDomainResponse() : base() { }
}
