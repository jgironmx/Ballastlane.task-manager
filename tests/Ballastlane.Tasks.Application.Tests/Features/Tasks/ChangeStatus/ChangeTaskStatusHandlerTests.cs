using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.ChangeStatus;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.ChangeStatus;

public class ChangeTaskStatusHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private readonly InMemoryTaskRepository _repository = new();
    private readonly FakeClock _clock = new(NowUtc);

    private ChangeTaskStatusHandler CreateHandler() => new(_repository, FakeCurrentUser.For(OwnerId), _clock);

    [Fact]
    public async Task HandleAsync_ShouldChangeStatus()
    {
        var task = TaskItem.Create(OwnerId, "Write report", null, null, NowUtc, Today);
        _repository.Add(task);

        var result = await CreateHandler().HandleAsync(
            new ChangeTaskStatusCommand(task.Id, TaskItemStatus.InProgress),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task HandleAsync_WhenTaskUnknown_ShouldReturnNotFound()
    {
        var result = await CreateHandler().HandleAsync(
            new ChangeTaskStatusCommand(Guid.NewGuid(), TaskItemStatus.Completed),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
