using Schmeconomics.Api.Secrets;

namespace Schmeconomics.Api.Tokens.AuthTokens;

public abstract class AuthTokenProviderException : Exception 
{
    protected AuthTokenProviderException(string? message) : base(message) { }
    protected AuthTokenProviderException(string? message, Exception? innerException) : base(message, innerException) { }

    public class JwtException(Exception ex) :
        AuthTokenProviderException("Failed to authorize JWT", ex),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status401Unauthorized;
        public string ClientMessage => Message;
        public string ServerMessage => ToString(); 
    }

    public class SecretProviderException (Secrets.SecretProviderException ex) : 
        AuthTokenProviderException($"An error occurred with the {nameof(ISecretsProvider)}", ex);

    public class TokenInvalidException() : AuthTokenProviderException("Token is not valid");
}
