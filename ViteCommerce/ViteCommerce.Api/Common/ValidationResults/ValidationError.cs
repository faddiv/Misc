namespace ViteCommerce.Api.Common.ValidationResults;

public class ValidationError
{
    public string Property { get; set; } = "";
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = "";
}
