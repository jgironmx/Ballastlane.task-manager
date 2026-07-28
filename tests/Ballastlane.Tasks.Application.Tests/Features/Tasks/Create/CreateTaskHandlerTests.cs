using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.Create;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.Create;

public class CreateTaskHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryTaskRepository _repository = new();
    private readonly FakeCurrentUser _currentUser = FakeCurrentUser.For(OwnerId);
    private readonly FakeClock _clock = new(NowUtc);

    private CreateTaskHandler CreateHandler() => new(_repository, _currentUser, _clock);

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldSucceed()
    {
        var command = new CreateTaskCommand("Write report", "Quarterly summary", null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Write report");
    }

    [Fact]
    public async Task HandleAsync_ShouldAssignOwnerIdFromCurrentUser()
    {
        var command = new CreateTaskCommand("Write report", null, null);

        await CreateHandler().HandleAsync(command, CancellationToken.None);

        _repository.Tasks.Single().OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetInitialStatusToPending()
    {
        var command = new CreateTaskCommand("Write report", null, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.Value.Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyTitle_ShouldFailWithValidation()
    {
        var command = new CreateTaskCommand(string.Empty, null, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        _repository.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithPastDueDate_ShouldFailWithValidation()
    {
        var today = DateOnly.FromDateTime(NowUtc.UtcDateTime);
        var command = new CreateTaskCommand("Write report", null, today.AddDays(-1));

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_WhenAnonymous_ShouldFailWithUnauthorized()
    {
        var handler = new CreateTaskHandler(_repository, FakeCurrentUser.Anonymous(), _clock);

        var result = await handler.HandleAsync(new CreateTaskCommand("Write report", null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
