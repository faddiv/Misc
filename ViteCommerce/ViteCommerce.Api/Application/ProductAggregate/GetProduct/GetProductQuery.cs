using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductAggregate.GetProduct;

public record GetProductQuery(
    string Id
    ) : IRequest<DomainResponse<Product>>;
