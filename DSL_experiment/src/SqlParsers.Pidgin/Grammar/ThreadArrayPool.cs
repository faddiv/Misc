using System.Buffers;
using Pidgin.Configuration;

namespace SqlParsers.Pidgin;

/// <summary>
/// An <see cref="IArrayPoolProvider"/> whose pools belong to the current thread and need no synchronization.
/// <para>
/// Pidgin rents an array every time a failing alternative records what it expected (its <c>PooledList</c>),
/// which happens many times per parse. <see cref="ArrayPool{T}.Shared"/> is thread-safe and clears the arrays
/// of reference-containing types on return; a plain per-thread stack of arrays is cheaper. One parse runs on
/// one thread, so the pool returned at the start of a parse is only used by that thread.
/// </para>
/// </summary>
internal sealed class ThreadArrayPoolProvider : IArrayPoolProvider
{
    public static readonly ThreadArrayPoolProvider Instance = new();

    public ArrayPool<T> GetArrayPool<T>() => ThreadArrayPool<T>.Current;
}

internal sealed class ThreadArrayPool<T> : ArrayPool<T>
{
    [ThreadStatic] private static ThreadArrayPool<T>? _current;

    public static ThreadArrayPool<T> Current => _current ??= new ThreadArrayPool<T>();

    // Bucket i holds arrays of length 16 << i.
    private const int BucketCount = 12;
    private const int MaxPerBucket = 32;
    private readonly T[][][] _buckets = new T[BucketCount][][];
    private readonly int[] _counts = new int[BucketCount];

    public override T[] Rent(int minimumLength)
    {
        var bucket = Bucket(minimumLength);
        if (bucket >= BucketCount) return new T[minimumLength];
        var count = _counts[bucket];
        if (count > 0)
        {
            _counts[bucket] = count - 1;
            return _buckets[bucket][count - 1];
        }
        return new T[16 << bucket];
    }

    public override void Return(T[] array, bool clearArray = false)
    {
        var bucket = Bucket(array.Length);
        if (bucket >= BucketCount || array.Length != 16 << bucket) return;
        var stack = _buckets[bucket] ??= new T[MaxPerBucket][];
        var count = _counts[bucket];
        if (count == MaxPerBucket) return;
        if (clearArray) Array.Clear(array);
        stack[count] = array;
        _counts[bucket] = count + 1;
    }

    private static int Bucket(int length) =>
        length <= 16 ? 0 : 32 - int.LeadingZeroCount(length - 1) - 4;
}
