namespace ViteCommerce.Api.Common.DomainAbstractions;

public static class DomainResponseExtensions
{
    public static async Task<IResult> ToCreatedResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task, Func<TDomainResponse, string> createResourceUrl)
    {
        var domainResult = await task;
        return domainResult
            .Match(
                response2 => Results.Created(createResourceUrl(response2), response2),
                ToBadRequest,
                Results.NoContent);
    }

    public static async Task<IResult> ToOkOrNotFoundResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        return domainResult
            .Match(
                Results.Ok,
                ToBadRequest,
                ToNotFound);
    }

    public static async Task<IResult> ToDeleteResult(
        this Task<DomainResponse<bool>> task)
    {
        var domainResult = await task;
        return domainResult
            .Match(
                found => found ? Results.NoContent() : Results.NotFound(),
                ToBadRequest,
                ToNotFound);
    }

    public static async Task<IResult> ToNoContentResult<TDomainResponse>(
        this Task<DomainResponse<TDomainResponse>> task)
    {
        var domainResult = await task;
        return domainResult
            .Match(
                Results.Ok,
                ToBadRequest,
                Results.NoContent);
    }


    private static IResult ToNotFound()
    {
        return Results.NotFound();
    }
    private static IResult ToBadRequest(Exception ex)
    {
        if (ex is ValidationFailedException vfe)
        {
            return
                Results.BadRequest(vfe.Errors);
        }

        return Results.BadRequest(new
        {
            Exception = ex.GetType().Name, ex.Message
        });
    }
}
