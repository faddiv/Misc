using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductAggregate.DeleteProduct;

public record DeleteTaskCommand(
    string Id
    ) : IRequest<DomainResponse<bool>>;
