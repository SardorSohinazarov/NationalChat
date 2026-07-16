using System.Text.Json.Serialization;

namespace API.DataTransferObjects.Responses;

public class Result
{
    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; init; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    protected Result(bool succeeded, string? message = null)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public static Result Fail() => new(false);
    public static Result Fail(string message) => new(false, message);

    public static Result Success() => new(true);
    public static Result Success(string message) => new(true, message);
    public static Result<T> Success<T>(T data) => Result<T>.Success(data);
}

public class Result<T> : Result
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    protected Result(bool succeeded, T? data = default, string? message = null)
        : base(succeeded, message)
    {
        Data = data;
    }

    public static new Result<T> Fail() => new(false);
    public static new Result<T> Fail(string message) => new(false, default, message);

    public static new Result<T> Success() => new(true);
    public static new Result<T> Success(string message) => new(true, default, message);
    public static Result<T> Success(T data) => new(true, data);
    public static Result<T> Success(T data, string message) => new(true, data, message);

    public static implicit operator Result<T>(T data) => Success(data);
}
