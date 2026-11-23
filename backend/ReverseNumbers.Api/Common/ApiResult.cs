namespace ReverseNumbers.Api.Common;

public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResult<T> Success(T data, string message = "success")
    {
        return new ApiResult<T> { Code = 0, Message = message, Data = data };
    }

    public static ApiResult<T> Fail(string message, int code = 1)
    {
        return new ApiResult<T> { Code = code, Message = message, Data = default };
    }
}

public class ApiResult : ApiResult<object?>
{
    public static ApiResult Success(string message = "success")
    {
        return new ApiResult { Code = 0, Message = message, Data = null };
    }

    public static new ApiResult Fail(string message, int code = 1)
    {
        return new ApiResult { Code = code, Message = message, Data = null };
    }
}
