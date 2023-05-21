namespace ViteCommerce.Api.Common.DomainAbstractions;

public interface IDomainResponse<T>
    where T : IDomainResponse<T>
{
    static abstract T ValidationFailed(IReadOnlyList<ValidationError> validationErrors);
}
