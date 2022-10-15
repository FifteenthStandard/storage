namespace FifteenthStandard.Storage;

public class FileBlobStore : IBlobStore
{
    public class Config
    {
        public string Root { get; init; } = "";
    }

    private readonly string _root;

    public FileBlobStore(string root)
    {
        _root = root;
    }

    public FileBlobStore(Config config)
        : this(config.Root)
    {
    }

    public async Task<byte[]?> GetBytesAsync(string path)
    {
        var file = BlobFile(path);

        if (!File.Exists(file)) return null;

        return await File.ReadAllBytesAsync(file);
    }

    public async Task PutBytesAsync(string path, byte[] contents)
    {
        var tmpFile = BlobFile(path, temp: true);

        Directory.CreateDirectory(Path.GetDirectoryName(tmpFile) ?? "");

        await File.WriteAllBytesAsync(tmpFile, contents);

        File.Move(tmpFile, BlobFile(path), true);
    }

    public Task RemoveAsync(string path)
    {
        if (File.Exists(path)) File.Delete(path);

        return Task.CompletedTask;
    }

    private string BlobFile(string hashKey, bool temp = false)
        => Path.Join(_root, $"{hashKey}{(temp ? ".tmp" : ".blob")}");
}