using Ballastlane.Tasks.Api.Contracts.Auth;
using Ballastlane.Tasks.Api.ErrorHandling;
using Ballastlane.Tasks.Application.Features.Authentication.GetCurrentUser;
using Ballastlane.Tasks.Application.Features.Authentication.Login;
using Ballastlane.Tasks.Application.Features.Authentication.Register;

namespace Ballastlane.Tasks.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequest request, RegisterUserHandler handler, CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(request.Email, request.Password, request.FirstName, request.LastName);
                var result = await handler.HandleAsync(command, cancellationToken);

                // Registration does not issue a token — the client must log in separately
                // afterwards (see docs/decisions/ADR-003-identity-jwt.md).
                return result.IsSuccess
                    ? Results.Created($"/api/auth/{result.Value.Id}", result.Value)
                    : result.Error.ToProblem();
            })
            .WithName("Register")
            .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, LoginUserHandler handler, CancellationToken cancellationToken) =>
            {
                var command = new LoginUserCommand(request.Email, request.Password);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(LoginResponse.FromResult(result.Value))
                    : result.Error.ToProblem();
            })
            .WithName("Login")
            .AllowAnonymous();

        group.MapGet("/me", async (GetCurrentUserHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
            })
            .WithName("GetCurrentUser")
            .RequireAuthorization();
    }
}
