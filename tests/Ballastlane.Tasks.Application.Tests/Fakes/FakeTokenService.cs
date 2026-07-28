using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Application.Tests.Fakes;

public sealed class FakeTokenService : ITokenService
{
    public AccessToken CreateToken(IdentityUserInfo user) =>
        new($"fake-token-for-{user.Id}", DateTimeOffset.UtcNow.AddMinutes(60));
}
