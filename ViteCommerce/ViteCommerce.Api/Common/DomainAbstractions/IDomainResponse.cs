namespace ViteCommerce.Api.Common.DomainAbstractions;

public interface IResponse<T>
    where T : IResponse<T>
{
    static abstract T ToFail(Exception exception);
}
