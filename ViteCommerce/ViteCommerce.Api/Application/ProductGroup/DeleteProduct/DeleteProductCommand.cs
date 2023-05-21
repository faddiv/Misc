using MediatR;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public record DeleteProductCommand(
    string Id
    ) : IRequest<DomainResponseBase<object>>;
