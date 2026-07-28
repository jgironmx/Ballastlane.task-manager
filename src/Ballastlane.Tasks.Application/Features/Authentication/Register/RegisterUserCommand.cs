namespace Ballastlane.Tasks.Application.Features.Authentication.Register;

public sealed record RegisterUserCommand(string Email, string Password, string FirstName, string LastName);
