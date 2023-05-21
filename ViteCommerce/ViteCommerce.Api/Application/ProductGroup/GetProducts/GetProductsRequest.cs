using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public record class GetProductsRequest : IRequest<DomainResponse<GetProductsResponse>>;
