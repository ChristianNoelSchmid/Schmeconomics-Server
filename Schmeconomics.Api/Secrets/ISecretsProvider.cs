namespace Schmeconomics.Api.Secrets;

public interface ISecretsProvider
{
    IAsyncEnumerable<byte[]> GetSecretsAsync(CancellationToken token = default);
}