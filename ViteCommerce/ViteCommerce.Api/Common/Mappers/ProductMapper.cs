using Catalog.API.Entities;
using Riok.Mapperly.Abstractions;
using ViteCommerce.Api.Application.ProductGroup.PostProduct;

namespace ViteCommerce.Api.Common.Mappers;

[Mapper]
public static partial class ProductMapper
{
    [MapperIgnoreTarget(nameof(Product.Id))]
    public static partial Product ToProduct(this PostProductCommand command);
}
