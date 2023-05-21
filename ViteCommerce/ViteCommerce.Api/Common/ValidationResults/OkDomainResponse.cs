namespace ViteCommerce.Api.Common.ValidationResults;

public sealed class OkDomainResponse<T> : DomainResponseBase<T>
{
    public static readonly OkDomainResponse<T> Instance = new();
    private OkDomainResponse() { }
}
