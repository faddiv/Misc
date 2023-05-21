using Mediator;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public record GetProductsQuery : IQuery<SelfContainedDomainResponse<GetProductsResponse>>;
