using MongoDB.Driver;

namespace Database
{
    public interface IUnitOfWork
    {
        ValueTask<IClientSessionHandle> GetSessionAsync();
    }
}
