namespace FifteenthStandard.Storage;

public class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly IDictionary<string, IDictionary<string, object>> _buckets;
    public InMemoryKeyValueStore()
    {
        _buckets = new Dictionary<string, IDictionary<string, object>>();
    }

    public Task<T?> GetAsync<T>(string hashKey, string sortKey)
        => Task.FromResult(
            GetBucket(hashKey).TryGetValue(sortKey, out var value)
                ? (T) value
                : default);

    public Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)
        => Task.FromResult(
            GetBucket(hashKey)
                .Where(kv => kv.Key.StartsWith(sortKeyPrefix))
                .OrderBy(kv => kv.Key)
                .Select(kv => (T)kv.Value));

    public Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)
        => Task.FromResult(
            GetBucket(hashKey)
                .Where(kv => LessThan(sortKeyStart, kv.Key, inclusive: true) && LessThan(kv.Key, sortKeyEnd))
                .OrderBy(kv => kv.Key)
                .Select(kv => (T)kv.Value));

    public Task<T> PutAsync<T>(string hashKey, string sortKey, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        GetBucket(hashKey)[sortKey] = value;
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string hashKey, string sortKey)
    {
        GetBucket(hashKey).Remove(sortKey);
        return Task.CompletedTask;
    }

    private IDictionary<string, object> GetBucket(string hashKey)
        => _buckets.TryGetValue(hashKey, out var bucket)
            ? bucket
            : (_buckets[hashKey] = new Dictionary<string, object>());

    private bool LessThan(string a, string b, bool inclusive = false)
    {
        var cmp = a.CompareTo(b);
        return cmp < 0 || (inclusive && cmp == 0);
    }
}
