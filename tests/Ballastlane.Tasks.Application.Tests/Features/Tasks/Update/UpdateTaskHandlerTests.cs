using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.Update;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.Update;

public class UpdateTaskHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private readonly InMemoryTaskRepository _repository = new();
    private readonly FakeClock _clock = new(NowUtc);

    private UpdateTaskHandler CreateHandler(Guid userId) => new(_repository, FakeCurrentUser.For(userId), _clock);

    private TaskItem SeedTask(Guid ownerId)
    {
        var task = TaskItem.Create(ownerId, "Original title", null, null, NowUtc, Today);
        _repository.Add(task);
        return task;
    }

    [Fact]
    public async Task HandleAsync_WhenOwner_ShouldUpdateOwnTask()
    {
        var task = SeedTask(OwnerId);
        var command = new UpdateTaskCommand(task.Id, "New title", "New description", Today.AddDays(5));

        var result = await CreateHandler(OwnerId).HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New title");
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherUser_ShouldReturnNotFound()
    {
        var task = SeedTask(OwnerId);
        var command = new UpdateTaskCommand(task.Id, "New title", null, null);

        var result = await CreateHandler(OtherUserId).HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTitle_ShouldFailWithValidation()
    {
        var task = SeedTask(OwnerId);
        var command = new UpdateTaskCommand(task.Id, string.Empty, null, null);

        var result = await CreateHandler(OwnerId).HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
