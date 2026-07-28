using Ballastlane.Tasks.Application.Contracts;
using Ballastlane.Tasks.Application.Features.Authentication.Login;

namespace Ballastlane.Tasks.Api.Contracts.Auth;

public sealed record LoginResponse(UserDto User, string AccessToken, string TokenType, DateTimeOffset ExpiresAtUtc)
{
    public static LoginResponse FromResult(LoginResult result) =>
        new(result.User, result.AccessToken, "Bearer", result.ExpiresAtUtc);
}
