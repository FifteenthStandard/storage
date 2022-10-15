namespace FifteenthStandard.Storage;

public interface IKeyValueStore
{
    Task<T?> GetAsync<T>(string hashKey, string sortKey);
    Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix);
    Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd);
    Task<T> PutAsync<T>(string hashKey, string sortKey, T value);
    Task RemoveAsync(string hashKey, string sortKey);
}
