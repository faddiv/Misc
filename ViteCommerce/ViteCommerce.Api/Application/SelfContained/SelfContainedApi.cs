using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Common;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.SelfContained;

public static class SelfContainedApi
{
    public static void Register(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/self-contained")
            .WithTags("SelfContainedApi")
            .WithOpenApi();

        group.MapPost("/validated", SelfContainedValidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

        group.MapPost("/un-validated", SelfContainedUnvalidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

    }

    public static async Task<IResult> SelfContainedValidated(
        ValidatedGet model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }

    public static async Task<IResult> SelfContainedUnvalidated(
        UnvalidatedGet model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }
}

public record class ValidatedGet(string Id) : ICommand<SelfContainedDomainResponse<ValidatedGetResponse>>;
public record class ValidatedGetResponse(string Id);

public class ValidatedGetValidator : AbstractValidator<ValidatedGet>
{
    public ValidatedGetValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}

public class ValidatedGetHandler : ICommandHandler<ValidatedGet, SelfContainedDomainResponse<ValidatedGetResponse>>
{
    public async ValueTask<SelfContainedDomainResponse<ValidatedGetResponse>> Handle(ValidatedGet command, CancellationToken cancellationToken)
    {
        await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (command.Id == "NoContent")
        {
            return DomainResponses.Ok<ValidatedGetResponse>();
        }
        else if (command.Id == "NotFound")
        {
            return DomainResponses.NotFound<ValidatedGetResponse>();
        }
        return DomainResponses.Wrap(new ValidatedGetResponse(command.Id));
    }
}


public record class UnvalidatedGet(string Id) : ICommand<SelfContainedDomainResponse<UnvalidatedGetResponse>>;
public record class UnvalidatedGetResponse(string Id);
public class UnvalidatedGetHandler : ICommandHandler<UnvalidatedGet, SelfContainedDomainResponse<UnvalidatedGetResponse>>
{
    

    public async ValueTask<SelfContainedDomainResponse<UnvalidatedGetResponse>> Handle(UnvalidatedGet command, CancellationToken cancellationToken)
    {
        await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (command.Id == "NoContent")
        {
            return DomainResponses.Ok<UnvalidatedGetResponse>();
        }
        else if (command.Id == "NotFound")
        {
            return DomainResponses.NotFound<UnvalidatedGetResponse>();
        }
        return DomainResponses.Wrap(new UnvalidatedGetResponse(command.Id));
    }
}
