namespace ViteCommerce.Api.Common.ValidationResults;

public sealed class NotFoundDomainResponse<T> : DomainResponseBase<T>
{
    public static readonly NotFoundDomainResponse<T> Instance = new();
    private NotFoundDomainResponse() : base() { }
}
