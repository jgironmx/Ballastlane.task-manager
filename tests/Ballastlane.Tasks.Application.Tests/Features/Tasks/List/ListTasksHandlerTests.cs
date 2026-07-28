using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Features.Tasks.List;
using Ballastlane.Tasks.Application.Tests.Fakes;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Features.Tasks.List;

public class ListTasksHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private readonly InMemoryTaskRepository _repository = new();

    private ListTasksHandler CreateHandler() => new(_repository, FakeCurrentUser.For(OwnerId));

    [Fact]
    public async Task HandleAsync_ShouldOnlyReturnCurrentUsersTasks()
    {
        _repository.Add(TaskItem.Create(OwnerId, "Mine", null, null, NowUtc, Today));
        _repository.Add(TaskItem.Create(OtherUserId, "Not mine", null, null, NowUtc, Today));

        var result = await CreateHandler().HandleAsync(new ListTasksQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(t => t.Title == "Mine");
    }

    [Fact]
    public async Task HandleAsync_WithStatusFilter_ShouldOnlyReturnMatchingTasks()
    {
        var pending = TaskItem.Create(OwnerId, "Pending task", null, null, NowUtc, Today);
        var inProgress = TaskItem.Create(OwnerId, "In progress task", null, null, NowUtc, Today);
        inProgress.ChangeStatus(TaskItemStatus.InProgress, NowUtc);
        _repository.Add(pending);
        _repository.Add(inProgress);

        var result = await CreateHandler().HandleAsync(new ListTasksQuery(Status: TaskItemStatus.InProgress), CancellationToken.None);

        result.Value.Items.Should().ContainSingle(t => t.Title == "In progress task");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WithNonPositivePage_ShouldReturnValidationError(int page)
    {
        var result = await CreateHandler().HandleAsync(new ListTasksQuery(Page: page), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task HandleAsync_WithOutOfRangePageSize_ShouldReturnValidationError(int pageSize)
    {
        var result = await CreateHandler().HandleAsync(new ListTasksQuery(PageSize: pageSize), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
