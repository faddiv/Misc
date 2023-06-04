using MediatR;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Application.ProductAggregate.DeleteProduct;
using ViteCommerce.Api.Application.ProductAggregate.GetProduct;
using ViteCommerce.Api.Application.ProductAggregate.GetProducts;
using ViteCommerce.Api.Application.ProductAggregate.PostProduct;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductAggregate
{
    public static class ProductApi
    {
        public static void Register(IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("/api/product")
                .WithTags("Product")
                .WithOpenApi();

            group.MapGet("/", GetProducts)
            .Produces(StatusCodes.Status200OK, typeof(GetProductsResponse));

            group.MapPost("/", CreateProduct)
            .Produces(StatusCodes.Status201Created, typeof(Product))
            .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

            group.MapGet("/{id}", GetProduct)
            .Produces(StatusCodes.Status200OK, typeof(Product))
            .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id}", DeleteProduct)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        }

        private static async Task<IResult> GetProducts(
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(new GetProductsRequest())
                .ToOkOrNotFoundResult();
        }

        private static async Task<IResult> GetProduct(
            [FromRoute] string id,
            [FromServices] IMediator mediator)
        {
            return await mediator.Send(new GetProductQuery(id))
                .ToOkOrNotFoundResult();
        }

        private static async Task<IResult> CreateProduct(
                [FromBody] PostProductCommand model,
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(model)
                .ToCreatedResult(r => $"/api/product/{r.Id}");
        }

        private static async Task<IResult> DeleteProduct(
                [FromRoute] string id,
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(new DeleteProductCommand(id))
                .ToDeleteResult();
        }
    }
}
