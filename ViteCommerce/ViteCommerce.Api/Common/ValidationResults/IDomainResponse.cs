using FluentValidation.Results;

namespace ViteCommerce.Api.Common.ValidationResults;

public interface IDomainResponse<T>
    where T : IDomainResponse<T>
{
    static abstract T ValidationFailed(ValidationResult result);
}
