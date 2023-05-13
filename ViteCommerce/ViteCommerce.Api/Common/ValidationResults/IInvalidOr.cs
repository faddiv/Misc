using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public interface IInvalidOr<T> where T : IInvalidOr<T>
{
    static abstract T Create(ValidationResult result);
}
