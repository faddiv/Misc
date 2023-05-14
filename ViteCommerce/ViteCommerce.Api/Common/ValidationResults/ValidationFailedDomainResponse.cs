using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public class ValidationFailedDomainResponse : IDomainResponse
{
    public ValidationFailedDomainResponse(ValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.IsValid)
            throw new ArgumentException("ValidationResult should have errors.");

        Errors = value.Errors.ConvertAll(e => new ValidationError
        {
            ErrorCode = e.ErrorCode,
            Message = e.ErrorMessage,
            Property = e.PropertyName
        }).AsReadOnly();
    }

    public IReadOnlyList<ValidationError> Errors { get; }
}
