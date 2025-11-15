namespace Schmeconomics.Api.JwtSecrets;

public interface ISecretsProvider
{
    IAsyncEnumerable<byte[]> GetSecretsAsync(CancellationToken token = default);
}