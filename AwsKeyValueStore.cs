using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;

namespace FifteenthStandard.Storage;

public class AwsKeyValueStore : IKeyValueStore
{
    public class Config
    {
        public string Table { get; set; } = "";
        public string Region { get; set; } = "";
        public string HashKeyName { get; set; } = "";
        public string SortKeyName { get; set; } = "";
    }

    private readonly Table _table;
    private readonly string _hashKeyName;
    private readonly string _sortKeyName;

    public AwsKeyValueStore(
        string table,
        string region,
        string hashKeyName,
        string sortKeyName)
    {
        _table = Table.LoadTable(
            new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(region)),
            table);
        _hashKeyName = hashKeyName;
        _sortKeyName = sortKeyName;
    }

    public AwsKeyValueStore(Config config)
        : this(config.Table, config.Region, config.HashKeyName, config.SortKeyName)
    {
    }

    public async Task<T?> GetAsync<T>(string hashKey, string sortKey)
    {
        var item = await _table.GetItemAsync(
            new Primitive(hashKey),
            new Primitive(sortKey));
        return item != null ? Convert<T>(item) : default;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)
    {
        var search = _table.Query(
            new Primitive(hashKey),
            new QueryFilter(
                _sortKeyName,
                QueryOperator.BeginsWith,
                new List<AttributeValue> { new AttributeValue(sortKeyPrefix) }));
        return (await search.GetRemainingAsync())
            .Select(Convert<T>);
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)
    {
        var search = _table.Query(
            new Primitive(hashKey),
            new QueryFilter(
                _sortKeyName,
                QueryOperator.Between,
                new List<AttributeValue> { new AttributeValue(sortKeyStart), new AttributeValue(sortKeyEnd) }));
        return (await search.GetRemainingAsync())
            .Select(Convert<T>);
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, int count)
    {
        var search = _table.Query(
            new Primitive(hashKey),
            new QueryFilter(
                _sortKeyName,
                QueryOperator.GreaterThanOrEqual,
                new List<AttributeValue> { new AttributeValue(sortKeyStart) }));
        var results = new List<T>();
        while (results.Count < count)
        {
            var set = await search.GetNextSetAsync();
            if (!set.Any()) break;
            results.AddRange(set.Take(count - results.Count).Select(Convert<T>));
        }
        return results;
    }

    public async Task<T> PutAsync<T>(string hashKey, string sortKey, T value)
    {
        await _table.PutItemAsync(Convert(hashKey, sortKey, value));
        return value;
    }

    public async Task RemoveAsync(string hashKey, string sortKey)
    {
        await _table.DeleteItemAsync(
            new Primitive(hashKey),
            new Primitive(sortKey));
    }

    private Document Convert<T>(string hashKey, string sortKey, T value)
    {
        var dict = JsonSerializer.Deserialize<IDictionary<string, object>>(
            JsonSerializer.Serialize(value)) ?? throw new Exception("Invalid item");
        dict[_hashKeyName] = hashKey;
        dict[_sortKeyName] = sortKey;
        var json = JsonSerializer.Serialize(dict);
        return Document.FromJson(json);
    }

    private T Convert<T>(Document document)
        => JsonSerializer.Deserialize<T>(document.ToJson())
            ?? throw new Exception("Invalid item");
}