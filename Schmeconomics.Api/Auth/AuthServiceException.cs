
namespace Schmeconomics.Api.Auth;

public abstract class AuthServiceError(string message) : Error(message)
{
    public class UserNotFound(string userName) :
        AuthServiceError($"User with name '{userName}' not found") { }

    public class PasswordVerificationFailed() :
        AuthServiceError("Password verification has failed");
}

public abstract class AuthServiceException : Exception
{
    /// <inheritdoc />
    public AuthServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    public AuthServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException ex)
        : AuthServiceException("An database error occurred", ex);

    public class AuthTokenProviderException(Tokens.AuthTokens.AuthTokenProviderException ex) 
        : AuthServiceException("An error occurred with the auth token provider", ex);

    public class RefreshTokenProviderException(Tokens.RefreshTokens.RefreshTokenProviderException ex)
        : AuthServiceException("An error occurred with the refresh token provider", ex);
}
