namespace Ballastlane.Tasks.Application.Common;

/// <summary>
/// Outcome of a use case that produces no value. Use cases return <see cref="Result"/>/
/// <see cref="Result{T}"/> instead of throwing for expected failures (validation, not-found,
/// unauthorized, conflict), so the API layer can map them to Problem Details without catching
/// exceptions for control flow. Generic success/failure factories live here (not on
/// <see cref="Result{T}"/> itself) to avoid static members on a generic type (CA1000).
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public UseCaseError Error { get; }

    protected internal Result(bool isSuccess, UseCaseError error)
    {
        if (isSuccess && error != UseCaseError.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == UseCaseError.None)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, UseCaseError.None);

    public static Result Failure(UseCaseError error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(true, value, UseCaseError.None);

    public static Result<T> Failure<T>(UseCaseError error) => new(false, default, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(bool isSuccess, T? value, UseCaseError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Only valid when <see cref="Result.IsSuccess"/> is <c>true</c>.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");
}
