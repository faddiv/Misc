using GreenDonutRelatedExperiments.NotificationCommon;

namespace GreenDonutRelatedExperiments.NotificationBenchParts;

public class Entry(PromiseCacheKey key, IPromise promise)
{
    private bool _initialized;
    private readonly Lock _lock = new();
    public PromiseCacheKey Key { get; } = key;
    public IPromise Promise { get; } = promise;

    public (bool newEntry, Promise<T> promise) EnsureInitialized<T>(PromiseCache cache, bool incrementUsage)
    {
        if (Promise is not Promise<T> promise)
        {
            throw new InvalidOperationException(
                $"Promise is not type of {typeof(Promise<T>).FullName}. Real type {Promise.GetType().FullName}");
        }

        if (_initialized)
        {
            return (false, promise);
        }

        bool notifySubscribers = false;
        lock (_lock)
        {
            if (_initialized)
            {
                return (false, promise);
            }

            if (!promise.IsClone)
            {
                notifySubscribers = true;
            }

            if (incrementUsage)
            {
                cache.IncrementInternal();
            }

            _initialized = true;
        }

        if (notifySubscribers)
        {
            promise.NotifySubscribersOnComplete(cache, Key);
        }

        return (true, promise);
    }
}
