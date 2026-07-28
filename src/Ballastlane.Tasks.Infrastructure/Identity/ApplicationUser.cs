using Microsoft.AspNetCore.Identity;

namespace Ballastlane.Tasks.Infrastructure.Identity;

/// <summary>
/// The ASP.NET Core Identity user. Lives in Infrastructure only — Domain and Application never
/// reference this type (see docs/decisions/ADR-003-identity-jwt.md).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
