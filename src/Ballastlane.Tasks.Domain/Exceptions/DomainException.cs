namespace Ballastlane.Tasks.Domain.Exceptions;

/// <summary>
/// Raised when an operation would violate a domain invariant (e.g. an invalid
/// <see cref="Tasks.TaskItem"/> title or due date). Application maps this to a safe,
/// user-facing validation error; it is never allowed to surface as an unhandled 500.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
