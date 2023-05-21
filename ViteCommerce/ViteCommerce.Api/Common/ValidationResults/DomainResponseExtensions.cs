namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponseExtensions
{
    public static async Task<IResult> ToCreatedResult<TDomainResponse>(
        this Task<DomainResponseBase<TDomainResponse>> task, Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        switch (domainResult)
        {
            case ValidationFailedDomainResponse<TDomainResponse> response:
                return Results.BadRequest(response.Errors);
            case NotFoundDomainResponse<TDomainResponse>:
                return Results.NotFound();
            case EmptyDomainResponse<TDomainResponse>:
                return Results.Ok();
            case OkDomainResponse<TDomainResponse> response2:
                return Results.Created(createResourceUrl(response2.Value), response2.Value);
            default:
                throw new ArgumentException($"Can't convert to IResult: {domainResult.GetType()}"); ;
        }
    }

    public static async Task<IResult> ToOkResult<TDomainResponse>(
        this Task<DomainResponseBase<TDomainResponse>> task)
    {
        var domainResult = await task;
        switch (domainResult)
        {
            case ValidationFailedDomainResponse<TDomainResponse> response:
                return Results.BadRequest(response.Errors);
            case NotFoundDomainResponse<TDomainResponse>:
                return Results.NotFound();
            case EmptyDomainResponse<TDomainResponse>:
                return Results.NoContent();
            case OkDomainResponse<TDomainResponse> response2:
                return Results.Ok(response2.Value);
            default:
                return Results.Ok(domainResult);
        }
    }

}
