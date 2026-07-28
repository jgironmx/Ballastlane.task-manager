using Ballastlane.Tasks.Application.Common;

namespace Ballastlane.Tasks.Application.Abstractions;

/// <summary>
/// Port over user account management. Application depends only on this abstraction — never on
/// <c>UserManager</c>/<c>SignInManager</c> — so use cases can be unit-tested without ASP.NET Core
/// Identity's infrastructure. The concrete implementation (backed by Identity) lives in
/// Infrastructure.
/// </summary>
public interface IIdentityService
{
    Task<Result<IdentityUserInfo>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken);

    /// <summary>Returns the user info on success, or <c>null</c> on invalid credentials.</summary>
    Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);

    Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
}
