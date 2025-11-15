
using System.Data.Common;

namespace Schmeconomics.Api.JwtSecrets;

public abstract class SecretProviderException : Exception
{
    /// <inheritdoc />
    public SecretProviderException(string? message) : base(message) { }
    /// <inheritdoc />
    public SecretProviderException(string? message, Exception? innerException) : base(message, innerException) { }
}

public class SecretProviderDbException(DbException dbException) 
    : SecretProviderException("A database error has ocurred", dbException);

public class SecretProviderNoSecretEntryFound()
    : SecretProviderException("No secret entries found in data source");