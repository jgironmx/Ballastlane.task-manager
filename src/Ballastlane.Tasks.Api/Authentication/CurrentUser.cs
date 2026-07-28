using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Api.Authentication;

/// <summary>
/// Implements <see cref="ICurrentUser"/> using <see cref="IHttpContextAccessor"/>. Lives in the
/// API project (not Infrastructure) because reading claims off <c>HttpContext.User</c> needs the
/// ASP.NET Core web framework, which the API project already has natively — see
/// docs/decisions/ADR-003-identity-jwt.md.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => UserId.HasValue;

    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // The JWT bearer handler (JsonWebTokenHandler, default since .NET 8) keeps the "sub"
            // claim type as-is; fall back to the legacy NameIdentifier mapping just in case.
            var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
    }
}
