namespace ViteCommerce.Api.Common.ValidationResults;

public class NotFoundDomainResponse : IDomainResponse
{
    public static readonly NotFoundDomainResponse Instance = new();
    private NotFoundDomainResponse() { }
}
