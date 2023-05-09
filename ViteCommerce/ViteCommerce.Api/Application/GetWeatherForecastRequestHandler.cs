using Mediator;

namespace ViteCommerce.Api.Application;

public class GetWeatherForecastRequestHandler : IQueryHandler<GetWeatherForecastRequest, WeatherForecast[]>
{
    static readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public ValueTask<WeatherForecast[]> Handle(GetWeatherForecastRequest query, CancellationToken cancellationToken)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
        return ValueTask.FromResult(forecast);
    }
}
