using Mediator;

namespace ViteCommerce.Api.Application;

public record GetWeatherForecastRequest : IQuery<WeatherForecast[]>;
