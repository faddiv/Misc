using MediatR;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public record GetProductQuery(
    string Id
    ) : IRequest<DomainResponse<Product>>;
