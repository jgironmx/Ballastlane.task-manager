namespace Ballastlane.Tasks.Application.Abstractions;

/// <summary>
/// A safe projection of an Identity user, carrying nothing Identity-internal (no password hash,
/// security stamp, or concurrency stamp) across the Application/Infrastructure boundary.
/// </summary>
public sealed record IdentityUserInfo(Guid Id, string Email, string FirstName, string LastName);
