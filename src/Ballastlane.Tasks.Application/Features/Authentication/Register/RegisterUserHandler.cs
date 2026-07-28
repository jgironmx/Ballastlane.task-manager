using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Authentication.Register;

public sealed class RegisterUserHandler(IIdentityService identityService)
{
    public async Task<Result<UserDto>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (!EmailValidator.IsValid(command.Email))
        {
            return Result.Failure<UserDto>(UseCaseError.Validation("register.email_invalid", "A valid email is required."));
        }

        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            return Result.Failure<UserDto>(UseCaseError.Validation("register.first_name_required", "First name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            return Result.Failure<UserDto>(UseCaseError.Validation("register.last_name_required", "Last name is required."));
        }

        // Password strength is delegated to ASP.NET Core Identity's configured policy.
        var creationResult = await identityService.CreateUserAsync(
            command.Email.Trim(),
            command.Password,
            command.FirstName.Trim(),
            command.LastName.Trim(),
            cancellationToken);

        if (creationResult.IsFailure)
        {
            return Result.Failure<UserDto>(creationResult.Error);
        }

        return Result.Success(UserDto.FromIdentityUser(creationResult.Value));
    }
}
