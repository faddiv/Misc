using Mediator;
using ViteCommerce.Api.Common;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public record PostProductCommand(
    string? Name,
    string? Category,
    string? Description,
    string? ImageFile,
    decimal? Price
    ) : ICommand<InvalidOr<PostProductResponse>>;
