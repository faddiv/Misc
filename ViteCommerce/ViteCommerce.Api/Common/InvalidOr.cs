using FluentValidation.Results;
using OneOf;

namespace ViteCommerce.Api.Common;

public class InvalidOr<T> : OneOfBase<T, ValidationResult>
{
    protected InvalidOr(OneOf<T, ValidationResult> input) : base(input)
    {
    }

    public static implicit operator InvalidOr<T>(T t) => new(t);
    public static implicit operator InvalidOr<T>(ValidationResult v) => new(v);

}
