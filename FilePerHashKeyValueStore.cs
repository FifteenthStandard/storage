using System.Text.Json;

namespace FifteenthStandard.Storage;

public class FilePerHashKeyValueStore : IKeyValueStore
{
    public class Config
    {
        public string Root { get; init; } = "";
    }

    private readonly string _root;

    public FilePerHashKeyValueStore(string root)
    {
        _root = root;
    }

    public FilePerHashKeyValueStore(Config config)
        : this(config.Root)
    {
    }

    public async Task<T?> GetAsync<T>(string hashKey, string sortKey)
        => (await LoadBucketAsync(hashKey))
            .TryGetValue(sortKey, out var value)
                ? (T) Convert<T>(value)
                : default;

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)
        => (await LoadBucketAsync(hashKey))
            .Where(kv => kv.Key.StartsWith(sortKeyPrefix))
            .OrderBy(kv => kv.Key)
            .Select(kv => (T)Convert<T>(kv.Value));

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)
        => (await LoadBucketAsync(hashKey))
            .Where(kv => LessThan(sortKeyStart, kv.Key, inclusive: true) && LessThan(kv.Key, sortKeyEnd))
            .OrderBy(kv => kv.Key)
            .Select(kv => (T)Convert<T>(kv.Value));

    public async Task<T> PutAsync<T>(string hashKey, string sortKey, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var bucket = await LoadBucketAsync(hashKey);
        bucket[sortKey] = value;
        await StoreBucketAsync(hashKey, bucket);
        return value;
    }

    public async Task RemoveAsync(string hashKey, string sortKey)
    {
        var bucket = await LoadBucketAsync(hashKey);
        bucket.Remove(sortKey);
        await StoreBucketAsync(hashKey, bucket);
    }

    private async Task<IDictionary<string, dynamic>> LoadBucketAsync(string hashKey)
    {
        var filename = HashFilename(hashKey);

        if (!File.Exists(filename)) return new Dictionary<string, dynamic>();

        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return (await JsonSerializer.DeserializeAsync<IDictionary<string, dynamic>>(stream))
                ?? new Dictionary<string, dynamic>();
        }
    }

    private async Task StoreBucketAsync(string hashKey, IDictionary<string, dynamic> bucket)
    {
        var tmpFilename = HashFilename(hashKey, temp: true);

        Directory.CreateDirectory(Path.GetDirectoryName(tmpFilename) ?? "");

        using (var stream = new FileStream(tmpFilename, FileMode.Create))
        {
            await JsonSerializer.SerializeAsync(stream, bucket);
        }

        File.Move(tmpFilename, HashFilename(hashKey), true);
    }

    private string HashFilename(string hashKey, bool temp = false)
        => Path.Join(_root, $"{hashKey}{(temp ? ".tmp" : ".json")}");

    private bool LessThan(string a, string b, bool inclusive = false)
    {
        var cmp = a.CompareTo(b);
        return cmp < 0 || (inclusive && cmp == 0);
    }

    private T Convert<T>(dynamic value)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
}