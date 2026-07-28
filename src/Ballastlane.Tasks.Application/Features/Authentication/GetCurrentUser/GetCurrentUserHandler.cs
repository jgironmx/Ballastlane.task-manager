using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserHandler(ICurrentUser currentUser, IIdentityService identityService)
{
    public async Task<Result<UserDto>> HandleAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<UserDto>(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        var user = await identityService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(UseCaseError.NotFound("auth.user_not_found", "The current user could not be found."));
        }

        return Result.Success(UserDto.FromIdentityUser(user));
    }
}
