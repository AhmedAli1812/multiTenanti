namespace HMS.SharedKernel.Primitives;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// Eliminates exception-as-control-flow anti-pattern for expected failures.
/// </summary>
public sealed class Result<T>
{
    private Result(T? value, Error? error, bool isSuccess)
    {
        Value     = value;
        Error     = error;
        IsSuccess = isSuccess;
    }

    public bool  IsSuccess { get; }
    public bool  IsFailure => !IsSuccess;
    public T?    Value     { get; }
    public Error? Error    { get; }

    public static Result<T> Success(T value)        => new(value, null, true);
    public static Result<T> Failure(Error error)    => new(default, error, false);
    public static Result<T> Failure(string message) => new(default, new Error(message), false);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

public sealed class Result
{
    private Result(Error? error, bool isSuccess)
    {
        Error     = error;
        IsSuccess = isSuccess;
    }

    public bool   IsSuccess { get; }
    public bool   IsFailure => !IsSuccess;
    public Error? Error     { get; }

    public static Result Success()              => new(null, true);
    public static Result Failure(Error error)   => new(error, false);
    public static Result Failure(string message)=> new(new Error(message), false);
}

public sealed record Error(string Message, string? Code = null)
{
    public static readonly Error None = new(string.Empty);
}
