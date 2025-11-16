namespace Schmeconomics.Api.Tokens.RefreshTokens;

public interface IRefreshTokenProvider
{
    Task<RefreshTokenFamilyModel> CreateNewTokenAsync(string userId, string ipAddress, CancellationToken stopToken = default);
    Task<RefreshTokenFamilyModel> CreateFromTokenAsync(string activeRefreshToken, string ipAddress, CancellationToken stopToken = default);
    Task RevokeTokenAsync(string token, string ipAddress, CancellationToken stopToken = default);
}
