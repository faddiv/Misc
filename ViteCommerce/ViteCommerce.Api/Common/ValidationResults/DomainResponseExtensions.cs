namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponseExtensions
{
    public static async ValueTask<IResult> ToOkResult(this ValueTask<IDomainResponse> task)
    {
        var domainResult = await task;
        if (domainResult is DomainResponseWrapper response2)
        {
            return Results.Ok(response2.Value);
        }
        else if (domainResult is ValidationFailedDomainResponse response)
        {
            return Results.BadRequest(response.Errors);
        }
        else if (domainResult is NotFoundDomainResponse)
        {
            return Results.NotFound();
        }
        else if (domainResult is OkDomainResponse)
        {
            return Results.NoContent();
        }
        else
        {
            return Results.Ok(domainResult);
        }
    }

    public static async ValueTask<IResult> ToCreatedResult<TDomainResponse>(this ValueTask<IDomainResponse> task,
        Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        if (domainResult is DomainResponseWrapper response2)
        {
            return Results.Created(createResourceUrl((TDomainResponse)response2.Value), response2.Value);
        }
        else if (domainResult is ValidationFailedDomainResponse response)
        {
            return Results.BadRequest(response.Errors);
        }
        else if (domainResult is NotFoundDomainResponse)
        {
            return Results.NotFound();
        }
        else if (domainResult is OkDomainResponse)
        {
            return Results.Ok();
        }
        else
        {
            return Results.Created(createResourceUrl((TDomainResponse)domainResult), domainResult);
        }
    }
}
