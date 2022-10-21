using System.Text.Json;
using Azure.Data.Tables;

namespace FifteenthStandard.Storage;

public class AzureKeyValueStore : IKeyValueStore
{
    public class Config
    {
        public string ConnectionString { get; set; } = "";
        public string Table { get; set; } = "";
    }

    private readonly TableClient _client;

    public AzureKeyValueStore(
        string connectionString,
        string table)
    {
        var serviceClient = new TableServiceClient(connectionString);
        _client = serviceClient.GetTableClient(table);
    }

    public AzureKeyValueStore(Config configuration)
        : this(configuration.ConnectionString, configuration.Table)
    {
    }

    public async Task<T?> GetAsync<T>(string hashKey, string sortKey)
    {
        var rowKey = RowKey(sortKey);
        try
        {
            var response = await _client.GetEntityAsync<TableEntity>(
                hashKey,
                rowKey);
            return Convert<T>(response.Value);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return default;
        }
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)
    {
        var rowKey = RowKey(sortKeyPrefix);
        var partitionFilter = $"PartitionKey eq '{hashKey}'";
        var rowFilter = $"RowKey ge '{rowKey}' and RowKey lt '{rowKey}|'";
        var response = _client.QueryAsync<TableEntity>(
            filter: $"{partitionFilter} and {rowFilter}");
        var results = new List<T>();
        await foreach (var page in response.AsPages())
        {
            foreach (var entity in page.Values)
            {
                var item = Convert<T>(entity);
                if (item != null) results.Add(item);
            }
        }
        return results;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)
    {
        var rowKeyStart = RowKey(sortKeyStart);
        var rowKeyEnd = RowKey(sortKeyEnd);
        var partitionFilter = $"PartitionKey eq '{hashKey}'";
        var rowFilter = $"RowKey ge '{rowKeyStart}' and RowKey lt '{rowKeyEnd}'";
        var response = _client.QueryAsync<TableEntity>(
            filter: $"{partitionFilter} and {rowFilter}");
        var results = new List<T>();
        await foreach (var page in response.AsPages())
        {
            foreach (var entity in page.Values)
            {
                var item = Convert<T>(entity);
                if (item != null) results.Add(item);
            }
        }
        return results;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, int count)
    {
        var ascending = count >= 0;
        count = Math.Abs(count);

        var rowKeyStart = RowKey(sortKeyStart);

        var partitionFilter = $"PartitionKey eq '{hashKey}'";
        var rowFilter = ascending
            ? $"RowKey ge '{rowKeyStart}'"
            : $"RowKey lt '{rowKeyStart}'";
        var response = _client.QueryAsync<TableEntity>(
            filter: $"{partitionFilter} and {rowFilter}",
            maxPerPage: count);
        var results = new List<T>();
        await foreach (var page in response.AsPages())
        {
            if (results.Count == count) break;
            foreach (var entity in page.Values)
            {
                if (results.Count == count) break;
                var item = Convert<T>(entity);
                if (item != null) results.Add(item);
            }
        }
        return results;
    }

    public async Task<T> PutAsync<T>(string hashKey, string sortKey, T value)
    {
        var entity = Convert(value);
        entity.PartitionKey = hashKey;
        entity.RowKey = RowKey(sortKey);
        await _client.UpsertEntityAsync<TableEntity>(entity);
        return value;
    }

    public async Task RemoveAsync(string hashKey, string sortKey)
    {
        await _client.DeleteEntityAsync(hashKey, RowKey(sortKey));
    }

    private string RowKey(string key) => key.Replace('/', '|');
    private T? Convert<T>(TableEntity entity)
    {
        var json = JsonSerializer.Serialize(entity);
        return JsonSerializer.Deserialize<T>(json);
    }
    private TableEntity Convert<T>(T value)
    {
        var json = JsonSerializer.Serialize(value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        return new TableEntity(dict);
    }
}
