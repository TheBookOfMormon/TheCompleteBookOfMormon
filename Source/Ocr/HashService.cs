using System.Security.Cryptography;

namespace Ocr;

internal class HashService
{
    public async ValueTask<byte[]> GetHashAsync(string filePath)
    {
        using var hasher = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        byte[] result = await hasher.ComputeHashAsync(stream);
        return result;
    }

    public byte[] GetHash(byte[] data)
    {
        using var hasher = SHA256.Create();
        byte[] result = hasher.ComputeHash(data);
        return result;
    }
}
