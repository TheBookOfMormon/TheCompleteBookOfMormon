using System.Collections.Concurrent;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public class AsyncWeakReferenceConcurrentDictionary<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
    where TValue : class
{
    private readonly ConcurrentDictionary<TKey, Task<WeakReference<TValue>>> Dictionary = new();

    public async Task<TValue> GetOrAddAsync(
        TKey key,
        Func<TKey, CancellationToken, Task<TValue>> valueFactory,
        CancellationToken cancellationToken)
    {
        async Task<(bool Found, TValue? Value)> getExistingValueAsync(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var existingEntry))
            {
                WeakReference<TValue>? weakReference = await existingEntry;
                if (weakReference != null && weakReference.TryGetTarget(out TValue? value))
                {
                    return (true, value);
                }
            }
            return (false, default);
        }

        (bool found, TValue? result) = await getExistingValueAsync(key);
        if (found)
            return result!;

        var task = new Lazy<Task<WeakReference<TValue>>>(async () =>
        {
            TValue newValue = await valueFactory(key, cancellationToken);
            return new WeakReference<TValue>(newValue);
        });

        var weakRefTask = (Task<WeakReference<TValue>>)Dictionary.GetOrAdd(key, _ => task.Value);
        WeakReference<TValue> weakRef = await weakRefTask;

        if (weakRef.TryGetTarget(out TValue? finalValue))
            return finalValue;

        // If the weak reference is dead, refresh the value.
        TValue freshValue = await valueFactory(key, cancellationToken);
        Dictionary[key] = Task.FromResult(new WeakReference<TValue>(freshValue));

        return freshValue;
    }

    public void Update(TKey key, TValue newValue)
    {
        var weakReference = new WeakReference<TValue>(newValue);
        Dictionary[key] = Task.FromResult(weakReference);
    }
}
