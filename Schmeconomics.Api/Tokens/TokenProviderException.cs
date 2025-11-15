using Schmeconomics.Api.JwtSecrets;

namespace Schmeconomics.Api.Tokens;

public abstract class TokenProviderException : Exception
{
    public TokenProviderException(string? message) : base(message) { }
    public TokenProviderException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException dbException)
        : TokenProviderException("A database error occured", dbException);

    public class TokenInvalidException()
        : TokenProviderException($"Token is not valid");

    public class JwtException(Exception exception)
        : TokenProviderException("A JWT exception occurred", exception);

    public class SecretProviderException(JwtSecrets.SecretProviderException jwtException)
        : TokenProviderException($"An error occurred with the {nameof(ISecretsProvider)}", jwtException);

    public class UserIdNotFoundException(string userId, string ipAddress)
        : TokenProviderException($"User ID {userId} does not exist. IP address: `{ipAddress}`");

    public class MalformedRefreshTokenException(string token, string ipAddress)
        : TokenProviderException($"Invalid refresh token: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenNotFoundException(string token, string ipAddress)
        : TokenProviderException($"Refresh token not found: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenStaleException(string token, string ipAddress)
        : TokenProviderException($"Refresh token is stale: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenDoesNotMatchException(string token, string ipAddress)
        : TokenProviderException($"Refresh token does not match: `{token}`. IP address: `{ipAddress}`");
}