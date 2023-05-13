namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public class GetProductResponse
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Category { get; set; } = "";

    public string? Description { get; set; } = "";

    public string? ImageFile { get; set; } = "";

    public decimal Price { get; set; }
}
