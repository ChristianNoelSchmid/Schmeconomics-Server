namespace Schmeconomics.Api.Tokens;

public static class TokenUtils
{
    /// <summary>
    /// Creates a random array of bytes of the provided size
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static byte[] CreateRandomBytes(uint size)
    {
        var bytes = new byte[size];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }
}