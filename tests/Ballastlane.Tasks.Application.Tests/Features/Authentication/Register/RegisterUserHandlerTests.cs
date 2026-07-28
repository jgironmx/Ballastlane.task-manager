using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Authentication.Register;
using Ballastlane.Tasks.Application.Tests.Fakes;

namespace Ballastlane.Tasks.Application.Tests.Features.Authentication.Register;

public class RegisterUserHandlerTests
{
    private readonly FakeIdentityService _identityService = new();

    private RegisterUserHandler CreateHandler() => new(_identityService);

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldSucceed()
    {
        var handler = CreateHandler();
        var command = new RegisterUserCommand("jane@example.com", "Password1", "Jane", "Doe");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEmail_ShouldFailWithConflict()
    {
        var handler = CreateHandler();
        await handler.HandleAsync(new RegisterUserCommand("jane@example.com", "Password1", "Jane", "Doe"), CancellationToken.None);

        var result = await handler.HandleAsync(new RegisterUserCommand("jane@example.com", "Password2", "Jane", "Doe"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_WithWeakPassword_ShouldFailWithValidationDetails()
    {
        var handler = CreateHandler();
        var command = new RegisterUserCommand("jane@example.com", "abc", "Jane", "Doe");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_ShouldFailWithValidation()
    {
        var handler = CreateHandler();
        var command = new RegisterUserCommand("not-an-email", "Password1", "Jane", "Doe");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_WithMissingFirstName_ShouldFailWithValidation()
    {
        var handler = CreateHandler();
        var command = new RegisterUserCommand("jane@example.com", "Password1", "  ", "Doe");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
