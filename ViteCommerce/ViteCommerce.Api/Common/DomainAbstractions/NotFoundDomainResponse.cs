namespace ViteCommerce.Api.Common.DomainAbstractions;

public sealed class NotFoundDomainResponse<T> : DomainResponse<T>
{
    public static readonly NotFoundDomainResponse<T> Instance = new();
    private NotFoundDomainResponse() : base() { }
}
