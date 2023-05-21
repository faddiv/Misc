namespace ViteCommerce.Api.Common.ValidationResults;

public static class DomainResponseExtensions
{
    public static async ValueTask<IResult> ToOkResult<TDomainResponse>(this ValueTask<SelfContainedDomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        switch (domainResult.StatusCode)
        {
            case DomainResponseStatus.Ok:
                return Results.Ok(domainResult.Value);
            case DomainResponseStatus.Failed:
                return Results.BadRequest(domainResult.Errors);
            case DomainResponseStatus.NoContent:
                return Results.NoContent();
            case DomainResponseStatus.NotFound:
                return Results.NotFound();
            default:
                throw new ArgumentException($"Unknown result: {domainResult.StatusCode}");
        }
    }

    public static async ValueTask<IResult> ToCreatedResult<TDomainResponse>(
        this ValueTask<SelfContainedDomainResponse<TDomainResponse>> task, Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        switch (domainResult.StatusCode)
        {
            case DomainResponseStatus.Ok:
                return Results.Created(createResourceUrl(domainResult.Value), domainResult.Value);
            case DomainResponseStatus.Failed:
                return Results.BadRequest(domainResult.Errors);
            case DomainResponseStatus.NoContent:
                return Results.NoContent();
            case DomainResponseStatus.NotFound:
                return Results.NotFound();
            default:
                throw new ArgumentException($"Unknown result: {domainResult.StatusCode}");
        }
    }

    public static async ValueTask<IResult> ToOkResult<TDomainResponse>(
        this ValueTask<DomainResponseBase<TDomainResponse>> task)
    {
        var domainResult = await task;
        switch (domainResult)
        {
            case ValidationFailedDomainResponse<TDomainResponse> response:
                return Results.BadRequest(response.Errors);
            case NotFoundDomainResponse<TDomainResponse>:
                return Results.NotFound();
            case OkDomainResponse<TDomainResponse>:
                return Results.NoContent();
            case DomainResponse<TDomainResponse> response2:
                return Results.Ok(response2.Value);
            default:
                return Results.Ok(domainResult);
        }
    }

    public static async ValueTask<IResult> ToCreatedResult<TDomainResponse>(
        this ValueTask<DomainResponseBase<TDomainResponse>> task, Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        switch (domainResult)
        {
            case ValidationFailedDomainResponse<TDomainResponse> response:
                return Results.BadRequest(response.Errors);
            case NotFoundDomainResponse<TDomainResponse>:
                return Results.NotFound();
            case OkDomainResponse<TDomainResponse>:
                return Results.Ok();
            case DomainResponse<TDomainResponse> response2:
                return Results.Created(createResourceUrl(response2.Value), response2.Value);
            default:
                throw new ArgumentException($"Can't convert to IResult: {domainResult.GetType()}"); ;
        }
    }
    public static async Task<IResult> ToOkResult<TDomainResponse>(this Task<SelfContainedDomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        switch (domainResult.StatusCode)
        {
            case DomainResponseStatus.Ok:
                return Results.Ok(domainResult.Value);
            case DomainResponseStatus.Failed:
                return Results.BadRequest(domainResult.Errors);
            case DomainResponseStatus.NoContent:
                return Results.NoContent();
            case DomainResponseStatus.NotFound:
                return Results.NotFound();
            default:
                throw new ArgumentException($"Unknown result: {domainResult.StatusCode}");
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
            case OkDomainResponse<TDomainResponse>:
                return Results.NoContent();
            case DomainResponse<TDomainResponse> response2:
                return Results.Ok(response2.Value);
            default:
                return Results.Ok(domainResult);
        }
    }

}
