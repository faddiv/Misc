using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public record GetProductQuery(
    string Id
    ) : IQuery<NotFoundOr<GetProductResponse>>;
