namespace Ecommerce.Application.Common;

public sealed class ServiceResult<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; } = 200;
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Data = data
    };

    public static ServiceResult<T> Fail(string error, int statusCode = 400) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Error = error
    };
}
