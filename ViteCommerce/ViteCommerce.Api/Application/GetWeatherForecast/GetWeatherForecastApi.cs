using MediatR;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;

namespace ViteCommerce.Api.Application.GetWeatherForecast;

public static class GetWeatherForecastApi
{

    public static void Register(IEndpointRouteBuilder app)
    {

        app.MapGet("/api/weatherforecast", async (
            HttpContext ctx,
            [FromServices] IMediator mediator,
            [FromServices] ILoggerFactory logger) =>
        {
            logger.CreateLogger("/api/weatherforecast").LogInformation("User: {User}", ctx.User);
            return await mediator.Send(new GetWeatherForecastRequest());
        })
            .WithName("GetWeatherForecast")
            .RequireAuthorization()
            .WithOpenApi();

        app.MapGet("/_configuration/{clientId}", (
            [FromRoute] string clientId,
            [FromServices] IClientRequestParametersProvider clientRequestParametersProvider,
            HttpContext context) =>
        {
            var parameters = clientRequestParametersProvider.GetClientParameters(context, clientId);
            return Results.Ok(parameters);
        })
            .WithName("GetClientParameters")
.WithOpenApi();
    }
}
