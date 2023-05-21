using MediatR;

namespace ViteCommerce.Api.Application.GetWeatherForecast;

public record GetWeatherForecastRequest : IRequest<DomainResponse<WeatherForecast[]>>;
