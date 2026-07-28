using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Authentication.Login;
using Ballastlane.Tasks.Application.Features.Authentication.Register;
using Ballastlane.Tasks.Application.Tests.Fakes;

namespace Ballastlane.Tasks.Application.Tests.Features.Authentication.Login;

public class LoginUserHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeTokenService _tokenService = new();

    private LoginUserHandler CreateHandler() => new(_identityService, _tokenService);

    private async Task RegisterUserAsync(string email, string password) =>
        await new RegisterUserHandler(_identityService)
            .HandleAsync(new RegisterUserCommand(email, password, "Jane", "Doe"), CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldSucceedAndReturnToken()
    {
        await RegisterUserAsync("jane@example.com", "Password1");
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new LoginUserCommand("jane@example.com", "Password1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.User.Email.Should().Be("jane@example.com");
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldFailWithGenericUnauthorized()
    {
        await RegisterUserAsync("jane@example.com", "Password1");
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new LoginUserCommand("jane@example.com", "WrongPassword"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        result.Error.Code.Should().Be("login.invalid_credentials");
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEmail_ShouldFailWithSameGenericError_AsWrongPassword()
    {
        await RegisterUserAsync("jane@example.com", "Password1");
        var handler = CreateHandler();

        var unknownEmailResult = await handler.HandleAsync(new LoginUserCommand("nobody@example.com", "Password1"), CancellationToken.None);
        var wrongPasswordResult = await handler.HandleAsync(new LoginUserCommand("jane@example.com", "WrongPassword"), CancellationToken.None);

        // Same error in both cases — the caller must not be able to tell whether the email exists.
        unknownEmailResult.Error.Should().Be(wrongPasswordResult.Error);
    }
}
