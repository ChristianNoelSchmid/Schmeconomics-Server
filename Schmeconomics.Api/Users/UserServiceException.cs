using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Api.Users;

public abstract class UserServiceException : Exception
{
    /// <inheritdoc />
    protected UserServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    protected UserServiceException(string? message, Exception? innerException) : base(message, innerException) { }
}

public class UserServiceNameReuseException(string reusedName) 
    : UserServiceException($"User with name {reusedName} already exists");

public class UserServiceDbException(DbUpdateException ex) 
    : UserServiceException("A database error has occurred", ex);