namespace ViteCommerce.Api.Common.ValidationResults;

public sealed class EmptyDomainResponse<T> : DomainResponseBase<T>
{
    public static readonly EmptyDomainResponse<T> Instance = new();
    private EmptyDomainResponse() { }
}
