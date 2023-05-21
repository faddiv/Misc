using ViteCommerce.Api.Common.DomainAbstractions;

public abstract class DomainResponse<T> : IDomainResponse<DomainResponse<T>>
{
    protected DomainResponse()
    {
    }

    public static DomainResponse<T> ValidationFailed(IReadOnlyList<ValidationError> validationErrors)
    {
        return new ValidationFailedDomainResponse<T>(validationErrors);
    }
}
