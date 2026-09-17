using System.Text.Json.Serialization;

namespace E_Commerce.Application.Shared.Models;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Mirrors the generic <see cref="Result{T}"/>.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }

    /// <summary>
    /// The <see cref="JsonConstructorAttribute"/> allows System.Text.Json to
    /// deserialize this type from cache by invoking the internal constructor
    /// directly. Without it, STJ cannot construct the type (no parameterless
    /// constructor, non-public parameterized constructor) and would fail on
    /// cache hit.
    /// </summary>
    [JsonConstructor]
    internal Result(bool succeeded, string[] errors)
    {
        Succeeded = succeeded;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result Success()
        => new(true, Array.Empty<string>());

    public static Result Failure(IEnumerable<string> errors)
        => new(false, errors.ToArray());

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

    /// <summary>
    /// The <see cref="JsonConstructorAttribute"/> allows System.Text.Json to
    /// deserialize this type from cache by invoking the internal constructor
    /// directly. Without it, STJ cannot construct the type (no parameterless
    /// constructor, non-public parameterized constructor) and would fail on
    /// cache hit.
    /// </summary>
    [JsonConstructor]
    internal Result(bool succeeded, T? data, string[] errors)
    {
        Succeeded = succeeded;
        Data = data;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result<T> Success(T data)
        => new(true, data, Array.Empty<string>());

    public static Result<T> Failure(IEnumerable<string> errors)
        => new(false, default, errors.ToArray());

    public static Result<T> Failure(string error)
        => new(false, default, new[] { error });
}