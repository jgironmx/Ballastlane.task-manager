using System.Net;
using Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

namespace Ballastlane.Tasks.Api.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(ApiTestGroup.Name)]
public class HealthEndpointTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task GetHealth_ShouldReturnOkWithHealthyStatus()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetHealth_ShouldBeAnonymous_NoAuthorizationHeaderRequired()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
