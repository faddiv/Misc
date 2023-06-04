using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductAggregate.DeleteProduct;

public record DeleteProductCommand(
    string Id
    ) : IRequest<DomainResponse<bool>>;
