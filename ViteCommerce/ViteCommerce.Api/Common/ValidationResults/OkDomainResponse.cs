namespace ViteCommerce.Api.Common.ValidationResults;

public class OkDomainResponse : IDomainResponse {
    public static readonly OkDomainResponse Instance = new();
    private OkDomainResponse() { }
}
