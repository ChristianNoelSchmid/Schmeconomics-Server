using System.Security.Claims;

namespace Schmeconomics.Api.Tokens;

public interface IAuthTokenProvider
{
    Task<string> CreateAuthTokenAsync(
        IDictionary<string, object> claims,
        CancellationToken token = default
    );
    Task<IReadOnlyList<Claim>> ValidateAuthTokenAsync(string token, CancellationToken cancelToken = default);
}