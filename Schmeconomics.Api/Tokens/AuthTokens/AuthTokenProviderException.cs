using Schmeconomics.Api.Secrets;

namespace Schmeconomics.Api.Tokens.AuthTokens;

public abstract class AuthTokenProviderException : Exception, IWebErrorInfo
{
    protected AuthTokenProviderException(string? message) : base(message) { }
    protected AuthTokenProviderException(string? message, Exception? innerException) : base(message, innerException) { }

    public abstract int StatusCode { get; }

    public class JwtException(Exception ex) : 
        AuthTokenProviderException("A JWT exception occurred", ex), 
        IWebErrorInfo
    {
        public override int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;
    }

    public class SecretProviderException (Secrets.SecretProviderException ex) : 
        AuthTokenProviderException($"An error occurred with the {nameof(ISecretsProvider)}", ex),
        IWebErrorInfo 
    {
        public string ServerMessage => Message;
        public override int StatusCode => StatusCodes.Status500InternalServerError;
    }

    public class TokenInvalidException() 
        : AuthTokenProviderException("Token is not valid"), IWebErrorInfo 
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;
        public string ClientMessage => Message;
    }
}
