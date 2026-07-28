using System.Net;
using System.Net.Http.Json;
using Ballastlane.Tasks.Api.Contracts.Auth;
using Ballastlane.Tasks.Api.IntegrationTests.Fixtures;
using Ballastlane.Tasks.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Tasks.Api.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(ApiTestGroup.Name)]
public class AuthEndpointTests(CustomWebApplicationFactory factory)
{
    private const string Password = "Password1!";

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Jane", "Doe"));

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Jane", "Doe"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        login!.AccessToken.Should().NotBeNullOrEmpty();
        login.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Jane", "Doe"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ShouldReturnProfile()
    {
        var (client, email) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnProblemDetailsBody()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problem.Type.Should().Be("urn:ballastlane-tasks:error:auth.required");
    }
}
