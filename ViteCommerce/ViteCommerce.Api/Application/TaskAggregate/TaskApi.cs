using MediatR;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Application.ProductAggregate.DeleteProduct;
using ViteCommerce.Api.Application.ProductAggregate.GetProduct;
using ViteCommerce.Api.Application.ProductAggregate.PostProduct;
using ViteCommerce.Api.Application.TaskAggregate.GetTasks;
using ViteCommerce.Api.Application.TaskAggregate.UpdateTask;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Models;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.TaskAggregate
{
    public static class TaskApi
    {
        public static void Register(IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("/api/task")
                .WithTags("Task")
                .WithOpenApi();

            group.MapGet("/", GetTasks)
            .Produces(StatusCodes.Status200OK, typeof(List<TaskModel>));

            group.MapPost("/", AddTask)
            .Produces(StatusCodes.Status201Created, typeof(TaskModel))
            .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

            group.MapPut("/", UpdateTask)
            .Produces(StatusCodes.Status200OK, typeof(TaskModel))
            .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>))
            .Produces(StatusCodes.Status404NotFound, typeof(List<ValidationError>));

            group.MapGet("/{id}", GetTask)
            .Produces(StatusCodes.Status200OK, typeof(TaskModel))
            .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id}", DeleteTask)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        }

        private static async Task<IResult> GetTasks(
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(new GetTasksRequest())
                .ToOkOrNotFoundResult();
        }

        private static async Task<IResult> GetTask(
            [FromRoute] string id,
            [FromServices] IMediator mediator)
        {
            return await mediator.Send(new GetTaskQuery(id))
                .ToOkOrNotFoundResult();
        }

        private static async Task<IResult> AddTask(
                [FromBody] AddTaskCommand model,
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(model)
                .ToCreatedResult(r => $"/api/task/{r.Id}");
        }

        private static async Task<IResult> UpdateTask(
                [FromBody] UpdateTaskCommand model,
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(model)
                .ToOkOrNotFoundResult();
        }

        private static async Task<IResult> DeleteTask(
                [FromRoute] string id,
                [FromServices] IMediator mediator)
        {
            return await mediator.Send(new DeleteTaskCommand(id))
                .ToDeleteResult();
        }
    }
}
