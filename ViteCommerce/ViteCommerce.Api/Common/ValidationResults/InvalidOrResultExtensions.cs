namespace ViteCommerce.Api.Common.ValidationResults
{
    public static class InvalidOrResultExtensions
    {
        public static async ValueTask<IResult> ToCreated<TResponse>(this ValueTask<InvalidOr<TResponse>> task,
            Func<TResponse, string> createResourceUrl)
        {
            var invalidOrResult = await task;
            if (invalidOrResult.IsValid)
                return Results.Created(createResourceUrl(invalidOrResult.Value), invalidOrResult.Value);
            return Results.BadRequest(invalidOrResult.Errors);
        }

        public static async ValueTask<IResult> ToOk<TResponse>(this ValueTask<NotFoundOr<TResponse>> task)
        {
            var invalidOrResult = await task;
            if (invalidOrResult.HasValue)
                return Results.Ok(invalidOrResult.Value);
            return Results.NotFound();
            
        }
    }
}
