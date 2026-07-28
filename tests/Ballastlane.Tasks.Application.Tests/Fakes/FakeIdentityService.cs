using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;

namespace Ballastlane.Tasks.Application.Tests.Fakes;

/// <summary>
/// In-memory stand-in for <see cref="IIdentityService"/>. Applies a minimal password rule
/// (length + digit) purely to exercise the "Identity validation errors are mapped safely" path —
/// the real policy is ASP.NET Core Identity's, configured and tested in Infrastructure.
/// </summary>
public sealed class FakeIdentityService : IIdentityService
{
    private readonly Dictionary<string, (Guid Id, string Password, string FirstName, string LastName)> _usersByEmail =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<Result<IdentityUserInfo>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        if (_usersByEmail.ContainsKey(email))
        {
            return Task.FromResult(Result.Failure<IdentityUserInfo>(
                UseCaseError.Conflict("register.email_taken", "An account with this email already exists.")));
        }

        var passwordErrors = ValidatePassword(password);
        if (passwordErrors.Count > 0)
        {
            return Task.FromResult(Result.Failure<IdentityUserInfo>(
                UseCaseError.Validation("register.invalid_password", "Password does not meet requirements.", passwordErrors)));
        }

        var id = Guid.NewGuid();
        _usersByEmail[email] = (id, password, firstName, lastName);

        return Task.FromResult(Result.Success(new IdentityUserInfo(id, email, firstName, lastName)));
    }

    public Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (!_usersByEmail.TryGetValue(email, out var user) || user.Password != password)
        {
            return Task.FromResult<IdentityUserInfo?>(null);
        }

        return Task.FromResult<IdentityUserInfo?>(new IdentityUserInfo(user.Id, email, user.FirstName, user.LastName));
    }

    public Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        foreach (var (email, user) in _usersByEmail)
        {
            if (user.Id == userId)
            {
                return Task.FromResult<IdentityUserInfo?>(new IdentityUserInfo(user.Id, email, user.FirstName, user.LastName));
            }
        }

        return Task.FromResult<IdentityUserInfo?>(null);
    }

    private static List<string> ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (password.Length < 6)
        {
            errors.Add("Passwords must be at least 6 characters.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Passwords must have at least one digit.");
        }

        return errors;
    }
}
