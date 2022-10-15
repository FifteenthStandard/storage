using System.Text;

namespace FifteenthStandard.Storage;

public interface IBlobStore
{
    Task<byte[]?> GetBytesAsync(string path);
    async Task<string?> GetStringAsync(string path)
    {
        var contents = await GetBytesAsync(path);
        return contents != null
            ? Encoding.UTF8.GetString(contents)
            : null;
    }
    Task PutBytesAsync(string path, byte[] contents);
    Task PutStringAsync(string path, string contents)
        => PutBytesAsync(path, Encoding.UTF8.GetBytes(contents));
    Task RemoveAsync(string path);
}
