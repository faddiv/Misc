using MongoDB.Driver;
using ViteCommerce.Api.Entities;

namespace Database;

public class ApplicationDbContext : IApplicationDbContext, IAsyncDisposable
{
    private readonly IMongoClient _client;
    private IMongoDatabase _database;
    private IClientSessionHandle? _session;

    public ApplicationDbContext(IMongoClient client)
    {
        _client = client;
        _database = client.GetDatabase("ViteCommerceDB");

        TaskItems = _database.GetCollection<TaskItem>("tasks");
    }

    public IMongoCollection<TaskItem> TaskItems { get; }

    public async ValueTask<IClientSessionHandle> GetSessionAsync(CancellationToken token = default)
    {
        if (_session is not null)
            return _session;

        _session = await _client.StartSessionAsync(cancellationToken: token);
        return _session;
    }

    public async ValueTask DisposeAsync()
    {
        if (_session is null)
            return;

        if (_session.IsInTransaction)
        {
            await _session.AbortTransactionAsync();
        }

        _session.Dispose();
        _session = null;
        GC.SuppressFinalize(this);
    }

}
