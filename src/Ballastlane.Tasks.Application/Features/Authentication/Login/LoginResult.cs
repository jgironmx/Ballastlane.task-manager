using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Authentication.Login;

public sealed record LoginResult(UserDto User, string AccessToken, DateTimeOffset ExpiresAtUtc);
