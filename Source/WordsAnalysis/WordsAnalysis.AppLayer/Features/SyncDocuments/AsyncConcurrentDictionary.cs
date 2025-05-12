using System.Collections.Concurrent;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public class AsyncConcurrentDictionary<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
    where TValue : class
{
    private readonly ConcurrentDictionary<TKey, Task<TValue>> Dictionary = new();

    public async Task<TValue> GetOrAddAsync(
        TKey key,
        Func<TKey, CancellationToken, Task<TValue>> valueFactory,
        CancellationToken cancellationToken)
    {
        async Task<TValue> createValueAsync(TKey k)
        {
            TValue newValue = await valueFactory(k, cancellationToken);
            return newValue;
        }

        var lazyTask = new Lazy<Task<TValue>>(() => createValueAsync(key));
        return await Dictionary.GetOrAdd(key, _ => lazyTask.Value);
    }

    public void Update(TKey key, TValue newValue)
    {
        Dictionary[key] = Task.FromResult(newValue);
    }
}