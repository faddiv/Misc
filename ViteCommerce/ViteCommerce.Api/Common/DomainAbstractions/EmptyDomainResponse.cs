namespace ViteCommerce.Api.Common.DomainAbstractions;

public sealed class EmptyDomainResponse<T> : DomainResponse<T>
{
    public static readonly EmptyDomainResponse<T> Instance = new();
    private EmptyDomainResponse() { }
}
