using System.Net;
using System.Net.Http.Json;
using Ballastlane.Tasks.Api.Contracts.Tasks;
using Ballastlane.Tasks.Api.IntegrationTests.Fixtures;
using Ballastlane.Tasks.Application.Contracts;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Api.IntegrationTests.Tasks;

[Trait("Category", "Integration")]
[Collection(ApiTestGroup.Name)]
public class TaskEndpointTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task CreateTask_WhenAuthenticated_ShouldReturnCreatedWithLocation()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Write report", "Quarterly summary", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var task = await response.Content.ReadFromJsonAsync<TaskDto>(TestJsonOptions.Default);
        task!.Title.Should().Be("Write report");
        task.Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public async Task CreateTask_WithInvalidTitle_ShouldReturnBadRequest()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest(string.Empty, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListTasks_ShouldOnlyReturnCurrentUsersTasks()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var (otherClient, _) = await factory.CreateAuthenticatedClientAsync();
        await ownerClient.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Owner task", null, null));
        await otherClient.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Other task", null, null));

        var response = await ownerClient.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>(TestJsonOptions.Default);
        page!.Items.Should().ContainSingle(t => t.Title == "Owner task");
        page.Items.Should().NotContain(t => t.Title == "Other task");
    }

    [Fact]
    public async Task ListTasks_WithoutPagingParameters_ShouldUseDefaultsAndReturnOk()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListTasks_WithNonPositivePage_ShouldReturnBadRequest(int page)
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/tasks?page={page}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ListTasks_WithOutOfRangePageSize_ShouldReturnBadRequest(int pageSize)
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/tasks?pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTaskById_WhenOwner_ShouldReturnTask()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(client, "Get me");

        var response = await client.GetAsync($"/api/tasks/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTaskById_WhenAnotherUsersTask_ShouldReturnNotFound()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var (otherClient, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(ownerClient, "Not yours");

        var response = await otherClient.GetAsync($"/api/tasks/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_WhenOwner_ShouldReturnOkWithUpdatedValues()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(client, "Original title");

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{created.Id}",
            new UpdateTaskRequest("New title", "New description", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>(TestJsonOptions.Default);
        updated!.Title.Should().Be("New title");
    }

    [Fact]
    public async Task UpdateTask_WithInvalidTitle_ShouldReturnBadRequest()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(client, "Original title");

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{created.Id}",
            new UpdateTaskRequest(string.Empty, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeTaskStatus_WhenOwner_ShouldReturnOkWithNewStatus()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(client, "Status me");

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{created.Id}/status",
            new ChangeTaskStatusRequest(TaskItemStatus.InProgress));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>(TestJsonOptions.Default);
        updated!.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task DeleteTask_WhenOwner_ShouldReturnNoContent_AndSubsequentGetReturnsNotFound()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateTaskAsync(client, "Delete me");

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<TaskDto> CreateTaskAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest(title, null, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>(TestJsonOptions.Default))!;
    }
}
