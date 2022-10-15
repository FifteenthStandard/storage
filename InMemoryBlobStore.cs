namespace FifteenthStandard.Storage;

public class InMemoryBlobStore : IBlobStore
{
    private IDictionary<string, byte[]> _blobs;

    public InMemoryBlobStore()
    {
        _blobs = new Dictionary<string, byte[]>();
    }

    public Task<byte[]?> GetBytesAsync(string path)
        => Task.FromResult(
            _blobs.TryGetValue(path, out var contents)
                ? contents
                : null);

    public Task PutBytesAsync(string path, byte[] contents)
    {
        _blobs[path] = contents;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string path)
    {
        _blobs.Remove(path);
        return Task.CompletedTask;
    }
}
