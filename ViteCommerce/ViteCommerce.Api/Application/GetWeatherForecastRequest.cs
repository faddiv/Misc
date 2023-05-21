using MediatR;

namespace ViteCommerce.Api.Application;

public record GetWeatherForecastRequest : IRequest<DomainResponseBase<WeatherForecast[]>>;
