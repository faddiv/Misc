using System.Buffers;
using System.Collections.Concurrent;
using GreenDonutRelatedExperiments.NotificationCommon;
using GreenDonutRelatedExperiments.NotificationV1;

namespace GreenDonutRelatedExperiments.NotificationBenchParts;

/// <summary>
/// A memorization cache for <c>DataLoader</c>.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PromiseCache"/>.
/// </remarks>
/// <param name="size">
/// The size of the cache. The minimum cache size is 10.
/// </param>
public sealed class PromiseCache(int size) : IPromiseCache
{
    private const int _minimumSize = 10;
    private readonly ConcurrentDictionary<PromiseCacheKey, Entry> _promises = new();
    private readonly ConcurrentDictionary<Type, List<Subscription>> _subscriptions = new();
    private readonly ConcurrentStack<IPromise> _promises2 = new();
    private readonly int _size = Math.Max(size, _minimumSize);
    private int _usage;

    /// <inheritdoc />
    public int Size => _size;

    /// <inheritdoc />
    public int Usage => _usage;

    /// <inheritdoc />
    public bool TryRemove(PromiseCacheKey key)
    {
        if (!_promises.TryRemove(key, out _))
        {
            return false;
        }

        IncrementInternal(-1);
        return true;
    }

    /// <inheritdoc />
    public void Publish<T>(T value)
    {
        var promise = Promise<T>.Create(value, cloned: true);

        _promises2.Push(promise);
        IncrementInternal();

        if (!_subscriptions.TryGetValue(typeof(T), out var subscriptions))
        {
            return;
        }

        List<Subscription> clone;
        lock (subscriptions)
        {
            clone = subscriptions.ToList();
        }
        foreach (var subscription in clone)
        {
            if (subscription is Subscription<T> casted)
            {
                casted.OnNext(promise);
            }
        }
    }

    /// <inheritdoc />
    public void PublishMany<T>(ReadOnlySpan<T> values)
    {
        var buffer = ArrayPool<IPromise>.Shared.Rent(values.Length);
        var span = buffer.AsSpan()[..values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            var promise = Promise<T>.Create(values[i], cloned: true);
            span[i] = promise;
        }

        _promises2.PushRange(buffer, 0, values.Length);
        IncrementInternal(values.Length);

        // now we notify all subscribers that are interested in the current promise type.
        if (_subscriptions.TryGetValue(typeof(T), out var subscriptions))
        {
            List<Subscription> clone;
            lock (subscriptions)
            {
                clone = subscriptions.ToList();
            }
            foreach (var subscription in clone)
            {
                if (subscription is not Subscription<T> casted)
                {
                    continue;
                }

                foreach (var item in span)
                {
                    casted.OnNext((Promise<T>)item);
                }
            }
        }

        ArrayPool<IPromise>.Shared.Return(buffer, true);
    }

    /// <inheritdoc />
    public IDisposable Subscribe<T>(Action<IPromiseCache, Promise<T>> next, string? skipCacheKeyType)
    {
        var type = typeof(T);
        var p1 = _promises2.ToArray();
        var p2 = _promises.ToArray();
        var subscriptions = _subscriptions.GetOrAdd(type, _ => []);
        var subscription = new Subscription<T>(this, subscriptions, next, skipCacheKeyType);

        lock (subscriptions)
        {
            subscriptions.Add(subscription);
        }

        foreach (var promise in p1.OfType<Promise<T>>())
        {
            subscription.OnNext(promise);
        }

        foreach (var keyValuePair in p2)
        {
            if (keyValuePair.Value.Promise is Promise<T> promise)
            {
                subscription.OnNext(keyValuePair.Key, promise);
            }
        }

        return subscription;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _promises.Clear();
        _promises2.Clear();
        _subscriptions.Clear();
        _usage = 0;
    }

    internal void NotifySubscribers<T>(in PromiseCacheKey key, in Promise<T> promise)
    {
        if (!_subscriptions.TryGetValue(typeof(T), out var subscriptions))
        {
            return;
        }

        var clonedPromise = promise.Clone();

        List<Subscription> clone;
        lock (subscriptions)
        {
            clone = [.. subscriptions];
        }

        foreach (var subscription in clone)
        {
            if (subscription is Subscription<T> casted)
            {
                casted.OnNext(key, clonedPromise);
            }
        }
    }

    public void IncrementInternal(int value = 1)
    {
        Interlocked.Add(ref _usage, value);
    }

}
