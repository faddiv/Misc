using MediatR;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public record DeleteProductCommand(
    string Id
    ) : IRequest<DomainResponse<object>>;
