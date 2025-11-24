namespace Schmeconomics.Api.Transactions;

public abstract class TransactionServiceError(string message) : Error(message)
{
    public class AccountNotFound(string accountId) :
        TransactionServiceError($"Account with id '{accountId}' not found") { }

    public class CategoryNotFound(string categoryId) :
        TransactionServiceError($"Category with id '{categoryId}' not found") { }

    public class TransactionNotFound(string transactionId) :
        TransactionServiceError($"Transaction with id '{transactionId}' not found") { }
}

public abstract class TransactionServiceException : Exception
{
    /// <inheritdoc />
    public TransactionServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    public TransactionServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException ex)
        : TransactionServiceException("An database error occurred", ex);
}
