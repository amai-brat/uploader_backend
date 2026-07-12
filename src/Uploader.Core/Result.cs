namespace Uploader.Core;

public class Result
{
    protected Result(bool isSuccess, string? errorMessage)
    {
        if (isSuccess && errorMessage != null || 
           !isSuccess && errorMessage == null)
        {
            throw new ArgumentException($"Invalid result creation => IsSuccess: {isSuccess}, Error: {errorMessage}");
        }
        
        IsSuccess = isSuccess;
        
    }
    
    public bool IsSuccess { get; }
    
    public bool IsFailure => !IsSuccess;
    
    public string? ErrorMessage { get; protected init; }
    
    public static Result Success() => new(true, null);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    
    public static Result Failure(string error) => new(false, error);
    public static Result<TValue> Failure<TValue>(string error) => new(default, false, error);
    
}

public class Result<TValue> : Result
{
    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("No access to value when result is failure");

    protected internal Result(TValue? value, bool isSuccess, string? errorMessage) : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}