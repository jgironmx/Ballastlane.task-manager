namespace Ballastlane.Tasks.Application.Common;

/// <summary>
/// A safe, serializable error description for a failed use case. <see cref="Details"/> carries
/// multiple sub-errors (e.g. several Identity password-policy violations at once) without ever
/// exposing internal exception messages or stack traces to the API layer.
/// </summary>
/// <remarks>Named <c>UseCaseError</c> rather than <c>Error</c> to avoid CA1716 (colliding with the
/// <c>Error</c> keyword/statement reserved in other .NET languages such as VB.NET).</remarks>
public sealed record UseCaseError(string Code, string Message, ErrorType Type, IReadOnlyList<string>? Details = null)
{
    public static readonly UseCaseError None = new(string.Empty, string.Empty, ErrorType.None);

    public static UseCaseError Validation(string code, string message, IReadOnlyList<string>? details = null) =>
        new(code, message, ErrorType.Validation, details);

    public static UseCaseError NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static UseCaseError Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static UseCaseError Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static UseCaseError Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
