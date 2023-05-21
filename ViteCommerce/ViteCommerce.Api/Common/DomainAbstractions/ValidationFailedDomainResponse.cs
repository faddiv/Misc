namespace ViteCommerce.Api.Common.DomainAbstractions;

public sealed class ValidationFailedDomainResponse<T> : DomainResponse<T>
{
    public ValidationFailedDomainResponse(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<ValidationError> Errors { get; }

}
