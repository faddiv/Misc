using GreenDonutRelatedExperiments.NotificationCommon;

namespace GreenDonutRelatedExperiments.NotificationBenchParts;

sealed class Subscription<T>(
    IPromiseCache owner,
    List<Subscription> subscriptions,
    Action<IPromiseCache, Promise<T>> next,
    string? skipCacheKeyType)
    : Subscription(subscriptions)
{
    public void OnNext(PromiseCacheKey key, Promise<T> promise)
    {
        if (promise.Task.IsCompletedSuccessfully &&
            skipCacheKeyType?.Equals(key.Type, StringComparison.Ordinal) != true)
        {
            next(owner, promise);
        }
    }

    public void OnNext(Promise<T> promise)
    {
        if (promise.Task.IsCompletedSuccessfully)
        {
            next(owner, promise);
        }
    }
}
