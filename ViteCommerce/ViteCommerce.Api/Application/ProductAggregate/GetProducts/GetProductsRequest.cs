using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductAggregate.GetProducts;

public record class GetProductsRequest : IRequest<DomainResponse<GetProductsResponse>>;
