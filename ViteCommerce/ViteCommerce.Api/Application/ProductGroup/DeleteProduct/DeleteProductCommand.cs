using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public record DeleteProductCommand(
    string Id
    ) : IRequest<DomainResponse<object>>;
