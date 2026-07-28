using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Authentication.Login;

public sealed class LoginUserHandler(IIdentityService identityService, ITokenService tokenService)
{
    private static readonly UseCaseError InvalidCredentials =
        UseCaseError.Unauthorized("login.invalid_credentials", "Invalid email or password.");

    public async Task<Result<LoginResult>> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        if (!EmailValidator.IsValid(command.Email) || string.IsNullOrEmpty(command.Password))
        {
            // Same generic failure as "credentials didn't match" — never reveal which part was wrong,
            // and never reveal whether the email exists.
            return Result.Failure<LoginResult>(InvalidCredentials);
        }

        var user = await identityService.ValidateCredentialsAsync(command.Email.Trim(), command.Password, cancellationToken);
        if (user is null)
        {
            return Result.Failure<LoginResult>(InvalidCredentials);
        }

        var token = tokenService.CreateToken(user);
        return Result.Success(new LoginResult(UserDto.FromIdentityUser(user), token.Value, token.ExpiresAtUtc));
    }
}
