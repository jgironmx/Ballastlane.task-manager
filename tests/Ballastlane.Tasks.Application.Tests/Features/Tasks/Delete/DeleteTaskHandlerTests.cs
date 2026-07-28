using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.Delete;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.Delete;

public class DeleteTaskHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private readonly InMemoryTaskRepository _repository = new();

    private DeleteTaskHandler CreateHandler(Guid userId) => new(_repository, FakeCurrentUser.For(userId));

    private TaskItem SeedTask(Guid ownerId)
    {
        var task = TaskItem.Create(ownerId, "Write report", null, null, NowUtc, Today);
        _repository.Add(task);
        return task;
    }

    [Fact]
    public async Task HandleAsync_WhenOwner_ShouldDeleteOwnTask()
    {
        var task = SeedTask(OwnerId);

        var result = await CreateHandler(OwnerId).HandleAsync(new DeleteTaskCommand(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherUser_ShouldReturnNotFound_AndNotDelete()
    {
        var task = SeedTask(OwnerId);

        var result = await CreateHandler(OtherUserId).HandleAsync(new DeleteTaskCommand(task.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        _repository.Tasks.Should().ContainSingle(t => t.Id == task.Id);
    }
}
