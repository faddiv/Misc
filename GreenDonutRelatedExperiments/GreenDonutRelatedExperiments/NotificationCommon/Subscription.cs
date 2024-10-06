namespace GreenDonutRelatedExperiments.NotificationCommon;

abstract class Subscription(
    List<Subscription> subscriptions)
    : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (subscriptions)
        {
            subscriptions.Remove(this);
        }

        _disposed = true;
    }
}
