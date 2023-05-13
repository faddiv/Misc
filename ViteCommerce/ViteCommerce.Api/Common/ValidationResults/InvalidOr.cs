using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;

namespace ViteCommerce.Api.Common.ValidationResults;

public class InvalidOr<T> : IInvalidOr<InvalidOr<T>>
{

    public InvalidOr(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        IsValid = true;
        Value = value;
    }
    public InvalidOr(ValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.IsValid)
            throw new ArgumentException("ValidationResult should have errors.");

        IsValid = false;
        Errors = value.Errors.ConvertAll(e => new ValidationError
        {
            ErrorCode = e.ErrorCode,
            Message = e.ErrorMessage,
            Property = e.PropertyName
        });
    }

    [MemberNotNull(nameof(Value))]
    [MemberNotNullWhen(false, nameof(Errors))]
    public bool IsValid { get; }

    public List<ValidationError>? Errors { get; }

    public T? Value { get; }

    public static InvalidOr<T> Create(ValidationResult result) => new(result);
    public static implicit operator InvalidOr<T>(T t) => new(t);
    public static implicit operator InvalidOr<T>(ValidationResult v) => new(v);

}
