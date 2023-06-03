using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.GetWeatherForecast;

public class GetWeatherForecastRequestHandler : IRequestHandler<GetWeatherForecastRequest, DomainResponse<WeatherForecast[]>>
{
    static readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public Task<DomainResponse<WeatherForecast[]>> Handle(GetWeatherForecastRequest request, CancellationToken cancellationToken)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
        return Task.FromResult(DomainResponses.OkOrEmpty(forecast));
    }
}
