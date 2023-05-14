using Mediator;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public record GetProductQuery(
    string Id
    ) : IQuery<IDomainResponse>;
