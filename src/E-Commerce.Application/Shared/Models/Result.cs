namespace E_Commerce.Application.Shared.Models;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Mirrors the generic <see cref="Result{T}"/>.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success()
        => new(true, Array.Empty<string>());

    public static Result Failure(IEnumerable<string> errors)
        => new(false, errors);

    public static Result Failure(string error)
        => new(false, new[] { error });
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
public class Result<T>
{
    public bool Succeeded { get; }
    public T? Data { get; }
    public string[] Errors { get; }

    internal Result(bool succeeded, T? data, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Data = data;
        Errors = errors.ToArray();
    }

    public static Result<T> Success(T data)
        => new(true, data, Array.Empty<string>());

    public static Result<T> Failure(IEnumerable<string> errors)
        => new(false, default, errors);

    public static Result<T> Failure(string error)
        => new(false, default, new[] { error });
}