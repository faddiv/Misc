using FluentValidation.Results;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Application.ProductGroup.GetProduct;
using ViteCommerce.Api.Application.ProductGroup.GetProducts;
using ViteCommerce.Api.Application.ProductGroup.PostProduct;
using ViteCommerce.Api.Common.ValidationResults;

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
            .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

            group.MapGet("/{id}", GetProduct)
            .WithName("GetProduct")
            .Produces(StatusCodes.Status201Created, typeof(GetProductResponse))
            .Produces(StatusCodes.Status404NotFound);

        }

        private static async Task<GetProductsResponse> GetProducts(
                [FromServices] IMediator mediator,
                [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(new GetProductsQuery());
        }

        private static async Task<IResult> GetProduct(
            [FromQuery] string id,
            [FromServices] IMediator mediator,
            [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(new GetProductQuery(id))
                .ToOk();
        }

        private static async Task<IResult> CreateProduct(
                [FromBody] PostProductCommand model,
                [FromServices] IMediator mediator,
                [FromServices] ILoggerFactory logger)
        {
            return await mediator.Send(model)
                .ToCreated(r => $"/api/product/{r.Id}");
        }
    }
}
