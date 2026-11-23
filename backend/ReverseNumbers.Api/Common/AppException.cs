namespace ReverseNumbers.Api.Common;

public class AppException : Exception
{
    public int Code { get; set; }

    public AppException(string message, int code = 1) : base(message)
    {
        Code = code;
    }
}
