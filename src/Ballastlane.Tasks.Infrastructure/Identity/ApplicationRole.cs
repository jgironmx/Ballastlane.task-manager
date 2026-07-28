using Microsoft.AspNetCore.Identity;

namespace Ballastlane.Tasks.Infrastructure.Identity;

/// <summary>
/// No roles are defined or assigned in this exercise; this type exists so Identity's role store
/// is available if a future increment needs role-based authorization.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>;
