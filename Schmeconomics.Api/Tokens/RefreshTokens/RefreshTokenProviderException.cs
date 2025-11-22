using Microsoft.AspNetCore.Diagnostics;

namespace Schmeconomics.Api.Tokens.RefreshTokens;

public abstract class RefreshTokenProviderException : Exception
{
    protected RefreshTokenProviderException(string? message) : base(message) { }
    protected RefreshTokenProviderException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException dbException) : 
        RefreshTokenProviderException("A database error occured", dbException);

    public class UserIdNotFound(string userId, string ipAddress) : 
        RefreshTokenProviderException($"User ID {userId} does not exist. IP address: {ipAddress}.");

    public class MalformedRefreshToken(string token, string ipAddress) : 
        RefreshTokenProviderException($"Malformed refresh token: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenNotFound(string token, string ipAddress) :
        RefreshTokenProviderException($"Refresh token not found: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenStale(string token, string ipAddress) : 
        RefreshTokenProviderException($"Refresh token is stale: `{token}`. IP address: `{ipAddress}`");

    public class RefreshTokenDoesNotMatch(string token, string ipAddress) : 
        RefreshTokenProviderException($"Refresh token does not match: `{token}`. IP address: `{ipAddress}`");
}
