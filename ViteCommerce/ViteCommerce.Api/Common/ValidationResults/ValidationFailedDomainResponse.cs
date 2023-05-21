namespace ViteCommerce.Api.Common.ValidationResults;

public sealed class ValidationFailedDomainResponse<T> : DomainResponseBase<T>
{
    public ValidationFailedDomainResponse(IReadOnlyList<ValidationError>? errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<ValidationError>? Errors { get; }

}
