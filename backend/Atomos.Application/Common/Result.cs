namespace Atomos.Application.Common;

public sealed class Result<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static Result<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static Result<T> Fail(string message, string code) => new()
    {
        Success = false,
        Message = message,
        Code = code
    };
}
