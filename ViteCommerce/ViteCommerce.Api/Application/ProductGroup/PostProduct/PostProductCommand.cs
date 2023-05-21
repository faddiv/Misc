using MediatR;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public record PostProductCommand(
    string? Name,
    string? Category,
    string? Description,
    string? ImageFile,
    decimal? Price
    ) : IRequest<DomainResponseBase<Product>>;
