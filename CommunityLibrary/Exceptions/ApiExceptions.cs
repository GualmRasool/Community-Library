using Microsoft.AspNetCore.Http;

namespace CommunityLibrary.Api.Exceptions;

/// <summary>
/// Base class for exceptions that map directly to a specific HTTP status code
/// and an ErrorResponse-shaped code/message pair.
/// </summary>
public abstract class ApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected ApiException(int statusCode, string errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class NotFoundApiException : ApiException
{
    public NotFoundApiException(string errorCode, string message)
        : base(StatusCodes.Status404NotFound, errorCode, message)
    {
    }
}

public class ConflictApiException : ApiException
{
    public ConflictApiException(string errorCode, string message)
        : base(StatusCodes.Status409Conflict, errorCode, message)
    {
    }
}

public class BadRequestApiException : ApiException
{
    public BadRequestApiException(string errorCode, string message)
        : base(StatusCodes.Status400BadRequest, errorCode, message)
    {
    }
}
