namespace Schmeconomics.Api.Secrets;

public abstract class SecretProviderException : Exception
{
    /// <inheritdoc />
    protected SecretProviderException(string? message) : base(message) { }
    /// <inheritdoc />
    protected SecretProviderException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException ex)
        : SecretProviderException("A database error has occurred", ex), IWebErrorInfo {
        public string ServerMessage => Message;
        public int StatusCode => StatusCodes.Status500InternalServerError;
    }

    public class NoSecretEntryFound() 
        : SecretProviderException("No secret entries found in data source") {
        public string ServerMessage => Message;
        public int StatusCode => StatusCodes.Status500InternalServerError;
    }
}