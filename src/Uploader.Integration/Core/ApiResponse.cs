namespace Uploader.Integration.Core;

public class ApiResponse
{
    public static ApiResponse<T> CreateSuccess<T>(T result, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = true,
            Result = result,
        };
    }
    
    public static ApiResponse<T> CreateFailure<T>(string errorMessage, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = false,
            ErrorMessage = errorMessage,
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public int StatusCode { get; set; }

    public bool IsSuccess { get; set; }
    
    public bool IsFailure => !IsSuccess;

    public T Result
    {
        get => IsSuccess 
            ? field
            : throw new InvalidOperationException("Result is failure");
        init => field = value;
    } = default!;

    public string ErrorMessage { get; set; } = null!;
}