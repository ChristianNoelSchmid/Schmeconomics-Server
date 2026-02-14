using System.Security.Claims;

namespace Schmeconomics.Api.Tokens.AuthTokens;

public interface IAuthTokenProvider
{
    Task<AuthTokenModel> CreateAuthTokenAsync(
        IDictionary<string, object> claims,
        CancellationToken token = default
    );
    Task<IReadOnlyList<Claim>> ValidateAuthTokenAsync(string token, CancellationToken cancelToken = default);
}