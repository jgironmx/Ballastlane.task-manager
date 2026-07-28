using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ballastlane.Tasks.Api.Contracts.Auth;

namespace Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

internal static class AuthTestHelper
{
    private const string Password = "Password1!";

    /// <summary>Registers a fresh user and returns an <see cref="HttpClient"/> pre-authenticated
    /// with their bearer token.</summary>
    public static async Task<(HttpClient Client, string Email)> CreateAuthenticatedClientAsync(this CustomWebApplicationFactory factory)
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var anonymousClient = factory.CreateClient();

        var registerResponse = await anonymousClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, Password, "Test", "User"));
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        return (client, email);
    }
}
