using FluentValidation.Results;

namespace ViteCommerce.Api.Common
{
    public static class OneOfExtensions
    {
        public static async ValueTask<IResult> MatchAsync<T1>(this ValueTask<InvalidOr<T1>> oneOf,
            Func<T1, IResult> func1, Func<ValidationResult, IResult> func2)
        {
            var oneOfResult = await oneOf;
            return oneOfResult.Match(func1, func2);
        }
    }
}
