namespace SharpWarden.WebClient.Exceptions;

class BitWardenHttpRequestException : HttpRequestException
{
    public ErrorType ErrorType;

    public BitWardenHttpRequestException(ErrorType errorType, string message):
        base(message)
    {
        ErrorType = errorType;
    }
}
