using Riok.Mapperly.Abstractions;
using ViteCommerce.Api.Application.ProductAggregate.PostProduct;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Common.Mappers;

[Mapper]
public static partial class ProductMapper
{
    [MapperIgnoreTarget(nameof(Product.Id))]
    public static partial Product ToProduct(this PostProductCommand command);
    
}
