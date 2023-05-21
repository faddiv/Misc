using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Common;

namespace ViteCommerce.Api.Application.MediatrSolution;

public class MediatrSolutionApi
{
    public static void Register(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/mediatr")
            .WithTags("MediatrApi")
            .WithOpenApi();

        group.MapPost("/validated", MediatrValidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse3))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

        group.MapPost("/un-validated", MediatrUnvalidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse3))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

    }

    public static async Task<IResult> MediatrValidated(
        ValidatedGet3 model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }

    public static async Task<IResult> MediatrUnvalidated(
        UnvalidatedGet3 model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }
}

public record class ValidatedGet3(string Id) : IRequest<DomainResponseBase<ValidatedGetResponse3>>;
public record class ValidatedGetResponse3(string Id);

public class ValidatedGetValidator3 : AbstractValidator<ValidatedGet3>
{
    public ValidatedGetValidator3()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}

public class ValidatedGetHandler : IRequestHandler<ValidatedGet3, DomainResponseBase<ValidatedGetResponse3>>
{
    public async Task<DomainResponseBase<ValidatedGetResponse3>> Handle(ValidatedGet3 request, CancellationToken cancellationToken)
    {
        //await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (request.Id == "NoContent")
        {
            return DomainResponses2.Ok<ValidatedGetResponse3>();
        }
        else if (request.Id == "NotFound")
        {
            return DomainResponses2.NotFound<ValidatedGetResponse3>();
        }
        return DomainResponses2.Wrap(new ValidatedGetResponse3(request.Id));
    }
}


public record class UnvalidatedGet3(string Id) : IRequest<SelfContainedDomainResponse<UnvalidatedGetResponse3>>;
public record class UnvalidatedGetResponse3(string Id);
public class UnvalidatedGetHandler : IRequestHandler<UnvalidatedGet3, SelfContainedDomainResponse<UnvalidatedGetResponse3>>
{
    public async Task<SelfContainedDomainResponse<UnvalidatedGetResponse3>> Handle(UnvalidatedGet3 request, CancellationToken cancellationToken)
    {
        //await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (request.Id == "NoContent")
        {
            return DomainResponses.Ok<UnvalidatedGetResponse3>();
        }
        else if (request.Id == "NotFound")
        {
            return DomainResponses.NotFound<UnvalidatedGetResponse3>();
        }
        return DomainResponses.Wrap(new UnvalidatedGetResponse3(request.Id));
    }
}
