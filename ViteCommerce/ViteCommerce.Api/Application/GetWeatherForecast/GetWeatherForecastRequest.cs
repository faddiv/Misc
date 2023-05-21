using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.GetWeatherForecast;

public record GetWeatherForecastRequest : IRequest<DomainResponse<WeatherForecast[]>>;
