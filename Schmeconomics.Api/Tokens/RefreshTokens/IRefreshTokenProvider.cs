namespace Schmeconomics.Api.Tokens.RefreshTokens;

public interface IRefreshTokenProvider
{
    Task<RefreshTokenResult> CreateNewTokenAsync(string userId, string ipAddress, CancellationToken stopToken = default);
    Task<RefreshTokenResult> CreateFromTokenAsync(string activeRefreshToken, string ipAddress, CancellationToken stopToken = default);
    Task RevokeTokenAsync(string token, string ipAddress, CancellationToken stopToken = default);
}