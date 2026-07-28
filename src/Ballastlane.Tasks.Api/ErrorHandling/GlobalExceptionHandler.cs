using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Tasks.Api.ErrorHandling;

/// <summary>
/// Catches anything that escapes an endpoint unhandled and returns a Problem Details response —
/// no stack trace or exception message is ever included in the response body, regardless of
/// environment. The real exception is logged server-side for diagnosis.
/// </summary>
public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandledException(logger, httpContext.Request.Method, httpContext.Request.Path.Value ?? string.Empty, exception);

        // BadHttpRequestException (e.g. a required query/route parameter missing or malformed)
        // is a transport/model-binding failure, not a server fault — surface it as 400, not 500.
        var isBadRequest = exception is BadHttpRequestException;
        var statusCode = isBadRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = isBadRequest ? "The request could not be processed." : "An unexpected error occurred.",
            Type = isBadRequest ? "urn:ballastlane-tasks:error:bad_request" : "urn:ballastlane-tasks:error:unexpected",
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, string method, string path, Exception exception);
}
