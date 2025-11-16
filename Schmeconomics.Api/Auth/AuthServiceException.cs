using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Tokens.RefreshTokens;
using Schmeconomics.Entities;

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

    public class DbException(System.Data.Common.DbException dbException) : 
        AuthServiceException("A database error occurred", dbException), 
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;
    }
}
