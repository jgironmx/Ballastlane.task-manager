using Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

namespace Ballastlane.Tasks.Api.IntegrationTests.Cors;

[Trait("Category", "Integration")]
[Collection(ApiTestGroup.Name)]
public class CorsPolicyTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task GetHealth_WithAngularDevOrigin_ShouldAllowCors()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowOriginValues).Should().BeTrue();
        allowOriginValues.Should().ContainSingle().Which.Should().Be("http://localhost:4200");
    }

    [Fact]
    public async Task GetHealth_WithUntrustedOrigin_ShouldNotAllowCors()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://evil.example.com");

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out _).Should().BeFalse();
    }
}
