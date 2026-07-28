using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace Ballastlane.Tasks.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="IIdentityService"/> against ASP.NET Core Identity's
/// <see cref="UserManager{TUser}"/>. Deliberately does not use <c>SignInManager&lt;TUser&gt;</c>:
/// it depends on ASP.NET Core's shared-framework HTTP/authentication-scheme types, which would
/// force this classlib to take a <c>FrameworkReference</c> on the whole web framework just for a
/// stateless JWT login check. <see cref="UserManager{TUser}"/>'s own lockout members
/// (<c>CheckPasswordAsync</c>/<c>AccessFailedAsync</c>/<c>IsLockedOutAsync</c>) provide the same
/// lockout behavior without that dependency. Never returns <see cref="ApplicationUser"/> or any
/// other Identity-internal type to Application — only the safe <see cref="IdentityUserInfo"/>
/// projection.
/// </summary>
public sealed class IdentityService(UserManager<ApplicationUser> userManager, IClock clock) : IIdentityService
{
    public async Task<Result<IdentityUserInfo>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return Result.Failure<IdentityUserInfo>(
                UseCaseError.Conflict("register.email_taken", "An account with this email already exists."));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAtUtc = clock.UtcNow,
        };

        var creationResult = await userManager.CreateAsync(user, password);
        if (!creationResult.Succeeded)
        {
            var details = creationResult.Errors.Select(e => e.Description).ToArray();
            return Result.Failure<IdentityUserInfo>(
                UseCaseError.Validation("register.invalid_password", "Password does not meet requirements.", details));
        }

        return Result.Success(ToUserInfo(user));
    }

    public async Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        if (userManager.SupportsUserLockout)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        return ToUserInfo(user);
    }

    public async Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToUserInfo(user);
    }

    private static IdentityUserInfo ToUserInfo(ApplicationUser user) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName);
}
