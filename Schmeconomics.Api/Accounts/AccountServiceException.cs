using System.Data.Common;

namespace Schmeconomics.Api.Accounts;

public abstract class AccountServiceError(string message) : Error(message)
{
    public class AccountNotFound(string accountId) :
        AccountServiceError($"Account with id '{accountId}' not found") { }

    public class UserNotFound(string userId) :
        AccountServiceError($"User with id '{userId}' not found") { }

    public class AccountAlreadyExists(string accountName) :
        AccountServiceError($"Account with name '{accountName}' already exists") { }
}

public abstract class AccountServiceException : Exception
{
    /// <inheritdoc />
    public AccountServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    public AccountServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException ex)
        : AccountServiceException("An database error occurred", ex);
}
