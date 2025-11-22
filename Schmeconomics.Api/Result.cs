using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Schmeconomics.Api;

public class Result<T>
{
    public T? Value { get; private set; }
    public Error? Error { get; private set; }

    public StackTrace StackTrace { get; }

    private Result() { StackTrace = new StackTrace(); }

    [MemberNotNull(nameof(Value))]
    public static Result<T> Ok(T value) => new() { Value = value };

    [MemberNotNull(nameof(Error))]
    public static Result<T> Err(Error error) => new() { Error = error };

    [MemberNotNull(nameof(Value))]
    public static implicit operator Result<T>(T value) => Ok(value);

    [MemberNotNull(nameof(Error))]
    public static implicit operator Result<T>(Error error) => Err(error);

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsOk => Value != null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => Error != null;
}

public class Result
{
    public Error? Error { get; private set; }
    private Result() {}

    public bool IsOk => Error == null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => Error != null;

    public static Result Ok() => new();

    [MemberNotNull(nameof(Error))]
    public static Result Err(Error error) => new() { Error = error };

    [MemberNotNull(nameof(Error))]
    public static implicit operator Result(Error error) => Err(error);
}

public abstract class Error(string message)
{
    public string Message { get; private set; } = message;
}