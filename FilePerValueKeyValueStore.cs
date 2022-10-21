using System.Text.Json;

namespace FifteenthStandard.Storage;

public class FilePerValueKeyValueStore : IKeyValueStore
{
    public class Config
    {
        public string Root { get; init; } = "";
    }

    private readonly string _root;

    public FilePerValueKeyValueStore(string root)
    {
        _root = root;
    }

    public FilePerValueKeyValueStore(Config config)
        : this(config.Root)
    {
    }

    public async Task<T?> GetAsync<T>(string hashKey, string sortKey)
    {
        var filename = Filename(hashKey, sortKey);
        if (!File.Exists(filename)) return default;

        return await GetFromFileAsync<T>(filename);
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)
    {
        var hashDirectory = HashDirectory(hashKey);
        if (!Directory.Exists(hashDirectory)) return Enumerable.Empty<T>();

        var files = Directory.GetFiles(hashDirectory, $"{sortKeyPrefix}*");

        var values = new List<T>();
        foreach (var file in files)
        {
            values.Add(await GetFromFileAsync<T>(file));
        }
        return values;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)
    {
        var hashDirectory = HashDirectory(hashKey);
        if (!Directory.Exists(hashDirectory)) return Enumerable.Empty<T>();

        var files = Directory.GetFiles(hashDirectory);

        var values = new List<T>();
        foreach (var file in files)
        {
            var currentSortKey = Path.GetFileNameWithoutExtension(file);

            if (!LessThan(currentSortKey, sortKeyEnd)) break;
            if (!LessThan(sortKeyStart, currentSortKey, true)) continue;

            values.Add(await GetFromFileAsync<T>(file));
        }
        return values;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, int count)
    {
        var ascending = count >= 0;
        count = Math.Abs(count);

        var hashDirectory = HashDirectory(hashKey);
        if (!Directory.Exists(hashDirectory)) return Enumerable.Empty<T>();

        var files = Directory.GetFiles(hashDirectory).AsEnumerable();

        if (ascending)
        {
            files = files.OrderBy(Path.GetFileNameWithoutExtension);
        }
        else
        {
            files = files.OrderByDescending(Path.GetFileNameWithoutExtension);
        }

        var values = new List<T>();
        foreach (var file in files)
        {
            if (values.Count == count) break;

            var currentSortKey = Path.GetFileNameWithoutExtension(file);

            if (ascending && !LessThan(sortKeyStart, currentSortKey, true)) continue;
            if (!ascending && !LessThan(currentSortKey, sortKeyStart)) continue;

            values.Add(await GetFromFileAsync<T>(file));
        }
        return values;
    }

    public async Task<T> PutAsync<T>(string hashKey, string sortKey, T value)
    {
        var tmpFilename = Filename(hashKey, sortKey, temp: true);

        Directory.CreateDirectory(Path.GetDirectoryName(tmpFilename) ?? "");

        using (var stream = new FileStream(tmpFilename, FileMode.Create))
        {
            await JsonSerializer.SerializeAsync(stream, value);
        }

        File.Move(tmpFilename, Filename(hashKey, sortKey), true);

        return value;
    }

    public Task RemoveAsync(string hashKey, string sortKey)
    {
        var filename = Filename(hashKey, sortKey);
        if (!File.Exists(filename)) return Task.CompletedTask;

        File.Delete(filename);
        return Task.CompletedTask;
    }

    private string HashDirectory(string hashKey)
        => Path.Join(_root, hashKey);

    private string Filename(string hashKey, string sortKey, bool temp = false)
        => Path.Join(_root, hashKey, $"{sortKey}{(temp ? ".tmp" : ".json")}");

    private bool LessThan(string a, string b, bool inclusive = false)
    {
        var cmp = a.CompareTo(b);
        return cmp < 0 || (inclusive && cmp == 0);
    }

    private async Task<T> GetFromFileAsync<T>(string filename)
    {
        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return await JsonSerializer.DeserializeAsync<T>(stream)
                ?? throw new InvalidOperationException("Failed to deserialize value.");
        }
    }

    private T Convert<T>(dynamic value)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
}