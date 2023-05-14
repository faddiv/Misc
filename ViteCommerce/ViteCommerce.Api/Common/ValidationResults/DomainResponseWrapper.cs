namespace ViteCommerce.Api.Common.ValidationResults;

public class DomainResponseWrapper : IDomainResponse
{
    public DomainResponseWrapper(object value)
    {
        Value = value;
    }

    public object Value { get; }
}
