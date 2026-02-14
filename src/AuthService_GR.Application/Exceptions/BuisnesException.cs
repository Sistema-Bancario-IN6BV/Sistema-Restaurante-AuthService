namespace AuthServiceIN6BV.Application.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCodes {get;}

    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCodes = errorCode;
    }

    public BusinessException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCodes = errorCode;
    }
}