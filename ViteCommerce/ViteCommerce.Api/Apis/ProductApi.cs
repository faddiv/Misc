using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Application;
using ViteCommerce.Api.Application.ProductGroup;
using ViteCommerce.Api.Application.ProductGroup.GetProducts;

namespace ViteCommerce.Api.Apis
{
    public static class ProductApi
    {
        public static void Register(IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("/api/product")
                //.WithGroupName("Product")
                .WithTags("Product")
                .WithOpenApi();

            group.MapGet("/", GetProduct)
            .WithName("GetProducts");
        }

        private static async Task<GetProductsResponse> GetProduct(
                [FromServices] IMediator mediator,
                [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(new GetProductsRequest());
        }
    }
}
