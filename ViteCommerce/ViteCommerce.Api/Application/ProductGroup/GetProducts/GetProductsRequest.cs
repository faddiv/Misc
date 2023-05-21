using MediatR;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public record class GetProductsRequest : IRequest<DomainResponse<GetProductsResponse>>;
