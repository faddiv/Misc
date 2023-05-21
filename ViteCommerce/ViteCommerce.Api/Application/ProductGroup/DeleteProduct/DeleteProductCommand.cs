using Mediator;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public record DeleteProductCommand(
    string Id
    ) : ICommand<SelfContainedDomainResponse<object>>;
