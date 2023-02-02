using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web;

internal class WebApis
{
    private static string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static WeatherForecast[] GetWeather()
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }

    public static async Task<IResult> PostOrder(
        string message,
        [FromServices] ILogger<WebApis> logger,
        [FromServices] IRequestClient<SubmitOrderBasic> requestClient)
    {
        logger.LogInformation("Request started");
        try
        {
            var response = await requestClient.GetResponse<OrderAccepted, OrderRejected>(new SubmitOrderBasic
            {
                CustomerNumber = message,
                OrderId = Guid.NewGuid(),
                Timestamp = DateTime.Now
            });
            var (accepted, rejected) = response;
            if (accepted.IsCompletedSuccessfully)
            {
                var acceptResult = await accepted;
                logger.LogInformation("Request succeed");
                return Results.Ok(response.Message);
            }
            if (rejected.IsCompletedSuccessfully)
            {
                var rejectedResult = await rejected;
                logger.LogInformation("Request rejected");
                return Results.BadRequest(response.Message);
            }
            return Results.NotFound();
        }
        catch (RequestFaultException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    public static async Task<IResult> SendOrder(
        string message,
        [FromServices] ILogger<WebApis> logger,
        [FromServices] IBus bus)
    {
        var ep = await bus.GetSendEndpoint(new Uri("exchange:ServiceBusBasedDotNet.Web.MessageContracts:PublishOrder"));
        await ep.Send(new PublishOrder
        {
            CustomerNumber = message,
            OrderId = Guid.NewGuid(),
            Timestamp = DateTime.Now
        });

        return Results.Ok();
    }

    public static async Task<IResult> PublishOrder(
        string message,
        [FromServices] ILogger<WebApis> logger,
        [FromServices] IPublishEndpoint endpoint)
    {
        await endpoint.Publish(new PublishOrder
        {
            CustomerNumber = message,
            OrderId = Guid.NewGuid(),
            Timestamp = DateTime.Now
        });

        return Results.Ok();
    }
    
    public static async Task<IResult> SubmitOrder(
        [FromServices] ILogger<WebApis> logger,
        [FromServices] IPublishEndpoint endpoint,
        [FromServices] IMessageDataRepository messageDataRepository,
        string customerNumber = "asdf123",
        string cardNumber = "456-852",
        string itemNumber = "231554",
        string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse condimentum convallis maximus. Nulla est mauris, faucibus eget vestibulum quis, imperdiet vitae dolor. Sed posuere, mauris et dignissim vehicula, neque mauris facilisis purus, a vestibulum mi sapien vel justo. In nisi turpis, gravida sit amet imperdiet non, faucibus vel est. Nam dictum commodo enim at dignissim. Praesent lacinia vel eros et blandit. Vivamus ornare cursus est.",
        int quantity = 1)
    {
        Guid correlationId = Guid.NewGuid();
        await endpoint.Publish(new SubmitOrder
        {
            CustomerNumber = customerNumber,
            OrderId = correlationId,
            Timestamp = DateTime.Now,
            CardNumber = cardNumber,
            ItemNumber = itemNumber,
            Quantity = quantity,
            Notes = await messageDataRepository.PutString(loremIpsum)
        });

        return Results.Ok(new { correlationId });
    }

    public static async Task<IResult> GetOrderState(
        Guid correlationId,
        [FromServices] IRequestClient<CheckOrder> requestClient)
    {
        var orderStatus = await requestClient.GetResponse<OrderStatus>(new CheckOrder
        {
            OrderId = correlationId
        });
        return Results.Ok(orderStatus);
    }

    public static async Task<IResult> DeleteCustomer(
        string customerNumber,
        [FromServices] IPublishEndpoint publishEndpoint)
    {
        await publishEndpoint.Publish(new CustomerAccountClosed
        {
            CustomerId = Guid.NewGuid(),
            CustomerNumber = customerNumber
        });
        return Results.Ok();
    }
}
