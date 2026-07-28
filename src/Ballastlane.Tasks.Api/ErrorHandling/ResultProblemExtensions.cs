using Ballastlane.Tasks.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Tasks.Api.ErrorHandling;

/// <summary>
/// Maps a failed <see cref="UseCaseError"/> to a Problem Details response. Centralizing this
/// keeps every endpoint's error contract identical (Part F5 of the backend sprint) and keeps
/// application-layer failures — validation, not-found, unauthorized, conflict — from ever needing
/// to be caught as exceptions at the API layer.
/// </summary>
public static class ResultProblemExtensions
{
    public static IResult ToProblem(this UseCaseError error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Message,
            // A non-resolving URN, not a fetchable URL — RFC 7807 only requires "type" to be a URI reference.
            Type = $"urn:ballastlane-tasks:error:{error.Code}",
        };

        if (error.Details is { Count: > 0 })
        {
            problemDetails.Extensions["errors"] = error.Details;
        }

        return Results.Problem(problemDetails);
    }
}
