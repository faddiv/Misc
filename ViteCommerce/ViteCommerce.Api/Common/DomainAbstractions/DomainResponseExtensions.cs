namespace ViteCommerce.Api.Common.DomainAbstractions;

public static class DomainResponseExtensions
{
    public static async Task<IResult> ToCreatedResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task, Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        return domainResult
            .OnSuccess(response2 => Results.Created(createResourceUrl(response2), response2))
            .OnEmpty(() => Results.NoContent())
            .OnFail<ValidationFailedException>((response) => Results.BadRequest(response.Errors))
            .ToValue();
    }

    public static async Task<IResult> ToOkOrNotFoundResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        return domainResult
            .OnSuccess(response2 => Results.Ok(response2))
            .OnEmpty(() => Results.NotFound())
            .OnFail<ValidationFailedException>((response) => Results.BadRequest(response.Errors))
            .ToValue();
    }

    public static async Task<IResult> ToDeleteResult(
        this Task<DomainResponse<bool>> task)
    {
        var domainResult = await task;
        return domainResult
            .OnSuccess(response2 => Results.NoContent())
            .OnEmpty(() => Results.NotFound())
            .OnFail<ValidationFailedException>((response) => Results.BadRequest(response.Errors))
            .ToValue();
    }

    public static async Task<IResult> ToNoContentResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        return domainResult
            .OnSuccess(response2 => Results.Ok(response2))
            .OnEmpty(() => Results.NoContent())
            .OnFail<ValidationFailedException>((response) => Results.BadRequest(response.Errors))
            .ToValue();
    }
}
