using MediatR;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application;

public class GetWeatherForecastRequestHandler : IRequestHandler<GetWeatherForecastRequest, DomainResponseBase<WeatherForecast[]>>
{
    static readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public Task<DomainResponseBase<WeatherForecast[]>> Handle(GetWeatherForecastRequest request, CancellationToken cancellationToken)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
        return Task.FromResult(DomainResponses.Wrap(forecast));
    }
}
