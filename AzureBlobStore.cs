using Azure.Storage.Blobs;

namespace FifteenthStandard.Storage;

public class AzureBlobStore : IBlobStore
{
    public class Config
    {
        public string ConnectionString { get; set; } = "";
        public string Container { get; set; } = "";
    }

    private readonly BlobContainerClient _client;

    public AzureBlobStore(
        string connectionString,
        string container)
    {
        var serviceClient = new BlobServiceClient(connectionString);
        _client = serviceClient.GetBlobContainerClient(container);
    }

    public AzureBlobStore(Config configuration)
        : this(configuration.ConnectionString, configuration.Container)
    {
    }

    public async Task<byte[]?> GetBytesAsync(string path)
    {
        var blobClient = _client.GetBlobClient(path);

        if (!(await blobClient.ExistsAsync())) return null;

        var resp = await blobClient.DownloadContentAsync();
        return resp.Value.Content.ToArray();
    }

    public async Task PutBytesAsync(string path, byte[] contents)
    {
        var blobClient = _client.GetBlobClient(path);

        var data = new BinaryData(contents);
        await blobClient.UploadAsync(data);
    }

    public async Task RemoveAsync(string path)
    {
        var blobClient = _client.GetBlobClient(path);

        await blobClient.DeleteIfExistsAsync();
    }
}