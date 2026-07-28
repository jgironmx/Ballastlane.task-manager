using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.GetById;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.GetById;

public class GetTaskByIdHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private readonly InMemoryTaskRepository _repository = new();

    private GetTaskByIdHandler CreateHandler(Guid userId) => new(_repository, FakeCurrentUser.For(userId));

    private TaskItem SeedTask(Guid ownerId)
    {
        var task = TaskItem.Create(ownerId, "Write report", null, null, NowUtc, Today);
        _repository.Add(task);
        return task;
    }

    [Fact]
    public async Task HandleAsync_WhenTaskBelongsToCurrentUser_ShouldReturnIt()
    {
        var task = SeedTask(OwnerId);

        var result = await CreateHandler(OwnerId).HandleAsync(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenTaskBelongsToAnotherUser_ShouldReturnNotFound()
    {
        var task = SeedTask(OtherUserId);

        var result = await CreateHandler(OwnerId).HandleAsync(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_WhenTaskDoesNotExist_ShouldReturnNotFound()
    {
        var result = await CreateHandler(OwnerId).HandleAsync(new GetTaskByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
