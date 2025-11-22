using System.Data.Common;

namespace Schmeconomics.Api.Users;

public abstract class UserServiceException : Exception
{
    /// <inheritdoc />
    protected UserServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    protected UserServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    public class NameReuse(string reusedName) : 
        UserServiceException($"User with name \"{reusedName}\" already exists"),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status409Conflict;
        public string ClientMessage => Message;
    }

    public class DbException(System.Data.Common.DbException ex) : 
        UserServiceException("A database error has occurred", ex),
        IWebErrorInfo
    {
        public int StatusCode => StatusCodes.Status500InternalServerError;
        public string ServerMessage => Message;
    }
}
