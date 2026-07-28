using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Authentication.GetCurrentUser;
using Ballastlane.Tasks.Application.Features.Authentication.Register;
using Ballastlane.Tasks.Application.Tests.Fakes;

namespace Ballastlane.Tasks.Application.Tests.Features.Authentication.GetCurrentUser;

public class GetCurrentUserHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeCurrentUser _currentUser = new();

    private GetCurrentUserHandler CreateHandler() => new(_currentUser, _identityService);

    [Fact]
    public async Task HandleAsync_WhenAuthenticated_ShouldReturnCurrentUserProfile()
    {
        var registerResult = await new RegisterUserHandler(_identityService)
            .HandleAsync(new RegisterUserCommand("jane@example.com", "Password1", "Jane", "Doe"), CancellationToken.None);
        _currentUser.UserId = registerResult.Value.Id;

        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task HandleAsync_WhenAnonymous_ShouldFailWithUnauthorized()
    {
        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
