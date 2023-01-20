using Amazon.S3;
using Amazon.S3.Model;

namespace FifteenthStandard.Storage;

public class AwsBlobStore : IBlobStore
{
    public class Config
    {
        public string Bucket { get; set; } = "";
        public string Region { get; set; } = "";
    }

    private readonly string _bucket;
    private readonly AmazonS3Client _client;

    public AwsBlobStore(
        string bucket,
        string region)
    {
        _bucket = bucket;
        _client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(region));
    }

    public AwsBlobStore(Config config)
        : this(config.Bucket, config.Region)
    {
    }

    public async Task<byte[]?> GetBytesAsync(string path)
    {
        GetObjectResponse response;
        try
        {
            response = await _client.GetObjectAsync(
                _bucket,
                path);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        using (var stream = new MemoryStream())
        {
            await response.ResponseStream.CopyToAsync(stream);
            return stream.ToArray();
        }
    }

    public async Task PutBytesAsync(string path, byte[] contents)
    {
        using (var stream = new MemoryStream(contents))
        {
            await _client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = path,
                    InputStream = stream
                });
        }
    }

    public async Task RemoveAsync(string path)
    {
        await _client.DeleteObjectAsync(
            _bucket,
            path);
    }
}