using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public class GetProductsResponse
{
    public required List<Product> Data { get; init; }
}
