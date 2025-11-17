
namespace Schmeconomics.Api.Auth;

public abstract class AuthServiceException : Exception
{
    protected AuthServiceException(string? message) : base(message) { }
    protected AuthServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class UserNotFoundException(string name) : 
        AuthServiceException($"User '{name}' not found"), 
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status404NotFound;
        public string ServerMessage => Message;
        public string ClientMessage => Message;
    }

    public class InvalidCredentialsException() : 
        AuthServiceException("Invalid credentials"), 
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status401Unauthorized;
        public string ServerMessage => Message;
        public string ClientMessage => Message;
    }

    public class AuthTokenProviderException(Tokens.AuthTokens.AuthTokenProviderException ex) :
        AuthServiceException("An error occurred with the auth token provider", ex),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;
    }

    public class RefreshTokenProviderException(Tokens.RefreshTokens.RefreshTokenProviderException ex) :
        AuthServiceException("An error occurred with the refresh token provider", ex),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;
    }

    public class DbException(System.Data.Common.DbException ex) : 
        AuthServiceException("A database error occurred", ex),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;

    }
}
