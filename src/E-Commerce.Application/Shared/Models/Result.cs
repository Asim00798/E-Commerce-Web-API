namespace E_Commerce.Application.Shared.Models;

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
    {
        return new Result<T>(true, data, Array.Empty<string>());
    }

    public static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors);
    }
}
