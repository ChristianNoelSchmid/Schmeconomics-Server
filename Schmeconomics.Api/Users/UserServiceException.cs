using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Schmeconomics.Api.Users;

public abstract class UserServiceException : Exception, IWebErrorInfo
{
    public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;
    
    /// <inheritdoc />
    protected UserServiceException(string? message) : base(message) { }

    /// <inheritdoc />
    protected UserServiceException(string? message, Exception? innerException) : base(message, innerException) { }

    int IWebErrorInfo.StatusCode => StatusCode;
    string IWebErrorInfo.ServerMessage => Message;
    string IWebErrorInfo.ClientMessage => Message;
}

public class UserServiceNameReuseException : UserServiceException
{
    private readonly string _reusedName;

    public UserServiceNameReuseException(string reusedName) 
        : base($"User with name {reusedName} already exists")
    {
        _reusedName = reusedName;
        StatusCode = StatusCodes.Status409Conflict;
    }
}

public class UserServiceDbException : UserServiceException
{
    public UserServiceDbException(DbUpdateException ex) 
        : base("A database error has occurred", ex)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}
