namespace NexCommerce.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ApiResponse<T> Ok(T data, string message = "")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? [] };

    public static ApiResponse<T> Fail(List<string> errors)
        => new() { Success = false, Errors = errors, Message = "One or more validation errors occurred." };
}

/// <summary>Non-generic convenience methods for operations that return no data payload.</summary>
public static class ApiResponse
{
    public static ApiResponse<object> OkNoData(string message = "")
        => ApiResponse<object>.Ok(null!, message);

    public static ApiResponse<object> Fail(string message, List<string>? errors = null)
        => ApiResponse<object>.Fail(message, errors);

    public static ApiResponse<object> Fail(List<string> errors)
        => ApiResponse<object>.Fail(errors);
}
