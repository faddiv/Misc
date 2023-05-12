using FluentValidation.Results;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Application.ProductGroup.GetProducts;
using ViteCommerce.Api.Application.ProductGroup.PostProduct;
using ViteCommerce.Api.Common;

namespace ViteCommerce.Api.Application.ProductGroup
{
    public static class ProductApi
    {
        public static void Register(IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("/api/product")
                //.WithGroupName("Product")
                .WithTags("Product")
                .WithOpenApi();

            group.MapGet("/", GetProducts)
            .WithName("GetProducts");

            group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .Produces(StatusCodes.Status201Created, typeof(PostProductResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ValidationResult));
        }

        private static async Task<GetProductsResponse> GetProducts(
                [FromServices] IMediator mediator,
                [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(new GetProductsRequest());
        }

        private static async Task<IResult> CreateProduct(
                [FromBody] PostProductCommand model,
                [FromServices] IMediator mediator,
                [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(model)
                .MatchAsync(
                    r => Results.Created($"/api/product/{r.Id}", r),
                    value => Results.BadRequest(value));
        }
    }
}
