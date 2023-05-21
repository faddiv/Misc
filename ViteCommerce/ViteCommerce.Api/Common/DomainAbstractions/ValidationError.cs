using FluentValidation.Results;

namespace ViteCommerce.Api.Common.DomainAbstractions;

public class ValidationError
{
    public string Property { get; set; } = "";
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = "";

    public static IReadOnlyList<ValidationError> Convert(ValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        if (value.IsValid)
            throw new ArgumentException("ValidationResult should have errors.");

        var validationErrors = value.Errors.ConvertAll(e => new ValidationError
        {
            ErrorCode = e.ErrorCode,
            Message = e.ErrorMessage,
            Property = e.PropertyName
        }).AsReadOnly();
        return validationErrors;
    }

}
