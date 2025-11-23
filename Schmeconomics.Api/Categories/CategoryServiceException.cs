using System.Data.Common;

namespace Schmeconomics.Api.Categories;

public abstract class CategoryServiceError(string message) : Error(message)
{
    public class AccountNotFound(string accountId) :
        CategoryServiceError($"Account with id '{accountId}' not found") { }

    public class CategoryNotFound(string categoryId) :
        CategoryServiceError($"Category with id '{categoryId}' not found") { }

    public class CategoryAlreadyExists(string categoryName) :
        CategoryServiceError($"Category with name '{categoryName}' already exists") { }

    public class MissingCategories() :
        CategoryServiceError("Not all categories for the account were included in the request") { }
}

public abstract class CategoryServiceException : Exception
{
    /// <inheritdoc />
    public CategoryServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    public CategoryServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class DbException(System.Data.Common.DbException ex)
        : CategoryServiceException("An database error occurred", ex);
}
