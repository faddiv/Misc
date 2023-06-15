// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;

namespace CachingSolutions;

interface ICustomCache
{
    Task<TValue?> GetOrCreate<TValue, TArgs>(
        string key, TArgs args, Func<IEntry, TArgs, Task<TValue?>> factory);
}

public interface IEntry
{
    string Key { get; }
}
class Entry<TValue> : IEntry
{
    public required string Key { get; init; }
    public Task<TValue?>? ValueAwaiter { get; set; }
    public TValue? Value { get; set; }
    public ValueTask<TValue?> GetValue()
    {
        if (ValueAwaiter is null)
        {
            return ValueTask.FromResult(Value);
        }
        return new ValueTask<TValue?>(GetValueInternal());
    }

    private async Task<TValue?> GetValueInternal()
    {
        if (ValueAwaiter is not null)
        {
            var factory = ValueAwaiter;
            if (factory is null)
            {
                return Value;
            }
            var result = await factory;
            Value = result;
            ValueAwaiter = null;
        }
        return Value;
    }
}
public class CustomCache : ICustomCache
{
    ConcurrentDictionary<string, IEntry> _items;

    public CustomCache()
    {
        _items = new();
    }
    public async Task<TValue?> GetOrCreate<TValue, TArgs>(
        string key, TArgs args, Func<IEntry, TArgs, Task<TValue?>> factory)
    {
        var entry = _items.GetOrAdd(key, (keyIn, args) =>
        {
            var entry = new Entry<TValue>
            {
                Key = keyIn,
            };
            entry.ValueAwaiter = Task.Run(() => factory(entry, args.args));
            return entry;
        }, (factory, args));

        if (entry is null)
        {
            throw new ApplicationException($"Entry creation failed for key: {key}");
        }
        if (entry is Entry<TValue> tentry)
        {
            return await tentry.GetValue();
        }
        throw new ApplicationException($"Tpype mismatch for entry. " +
            $"Key {key} " +
            $"Expected type: {GetValueType(entry)} " +
            $"Actual type: {typeof(TValue)}");
    }

    private Type GetValueType(IEntry entry)
    {
        return entry.GetType().GenericTypeArguments.FirstOrDefault()
            ?? typeof(void);
    }
}
