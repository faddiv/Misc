using MongoDB.Driver;

namespace Database;

public class UnitOfWork : IAsyncDisposable, IUnitOfWork
{

    private readonly IMongoClient _client;
    private IClientSessionHandle? _session;

    public UnitOfWork(IMongoClient client)
    {
        _client = client;

    }

    public IClientSessionHandle Session =>
        _session ?? throw new InvalidOperationException("The session is not created.");

    public async ValueTask<IClientSessionHandle> GetSessionAsync()
    {
        if (_session is not null)
            return _session;

        _session = await _client.StartSessionAsync();
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
