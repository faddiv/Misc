using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ViteCommerce.Api.Common;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.Subclassing;

public static class SubclassingApi
{
    public static void Register(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/sub-classing")
            .WithTags("SubbclassingApi")
            .WithOpenApi();

        group.MapPost("/validated", SubClassingValidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse2))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

        group.MapPost("/un-validated", SubClassingUnvalidated)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status201Created, typeof(ValidatedGetResponse2))
        .Produces(StatusCodes.Status400BadRequest, typeof(List<ValidationError>));

    }

    public static async Task<IResult> SubClassingValidated(
        ValidatedGet2 model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }

    public static async Task<IResult> SubClassingUnvalidated(
        UnvalidatedGet2 model,
        [FromServices] IMediator mediator)
    {
        return await mediator.Send(model)
            .ToOkResult();
    }
}

public record class ValidatedGet2(string Id) : ICommand<DomainResponseBase<ValidatedGetResponse2>>;
public record class ValidatedGetResponse2(string Id);

public class ValidatedGetValidator2 : AbstractValidator<ValidatedGet2>
{
    public ValidatedGetValidator2()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}

public class ValidatedGetHandler : ICommandHandler<ValidatedGet2, DomainResponseBase<ValidatedGetResponse2>>
{
    public async ValueTask<DomainResponseBase<ValidatedGetResponse2>> Handle(ValidatedGet2 command, CancellationToken cancellationToken)
    {
        //await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (command.Id == "NoContent")
        {
            return DomainResponses2.Ok<ValidatedGetResponse2>();
        }
        else if (command.Id == "NotFound")
        {
            return DomainResponses2.NotFound<ValidatedGetResponse2>();
        }
        return DomainResponses2.Wrap(new ValidatedGetResponse2(command.Id));
    }
}


public record class UnvalidatedGet2(string Id) : ICommand<SelfContainedDomainResponse<UnvalidatedGetResponse2>>;
public record class UnvalidatedGetResponse2(string Id);
public class UnvalidatedGetHandler : ICommandHandler<UnvalidatedGet2, SelfContainedDomainResponse<UnvalidatedGetResponse2>>
{
    public async ValueTask<SelfContainedDomainResponse<UnvalidatedGetResponse2>> Handle(UnvalidatedGet2 command, CancellationToken cancellationToken)
    {
        //await Task.Delay(Contants.Delay).ConfigureAwait(false);
        if (command.Id == "NoContent")
        {
            return DomainResponses.Ok<UnvalidatedGetResponse2>();
        }
        else if (command.Id == "NotFound")
        {
            return DomainResponses.NotFound<UnvalidatedGetResponse2>();
        }
        return DomainResponses.Wrap(new UnvalidatedGetResponse2(command.Id));
    }
}

