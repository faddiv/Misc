namespace ViteCommerce.Api.Common.ValidationResults
{
    public interface INotFoundOr<T> where T : INotFoundOr<T>
    {
        static abstract T NotFoundResponse();
    }
}
