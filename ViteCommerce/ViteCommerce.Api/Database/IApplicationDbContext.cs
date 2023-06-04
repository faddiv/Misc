using MongoDB.Driver;
using ViteCommerce.Api.Entities;

namespace Database
{
    public interface IApplicationDbContext
    {
        ValueTask<IClientSessionHandle> GetSessionAsync(CancellationToken token = default);
        IMongoCollection<TaskItem> TaskItems { get; }
    }
}
