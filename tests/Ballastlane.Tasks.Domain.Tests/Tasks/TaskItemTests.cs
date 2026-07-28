using Ballastlane.Tasks.Domain.Exceptions;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Domain.Tests.Tasks;

public class TaskItemTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTimeOffset NowUtc = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    private static TaskItem CreateValidTask(
        string title = "Write report",
        string? description = "Quarterly summary",
        DateOnly? dueDate = null) =>
        TaskItem.Create(OwnerId, title, description, dueDate, NowUtc, Today);

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var task = CreateValidTask(dueDate: Today.AddDays(3));

        task.Id.Should().NotBe(Guid.Empty);
        task.OwnerId.Should().Be(OwnerId);
        task.Title.Should().Be("Write report");
        task.Description.Should().Be("Quarterly summary");
        task.DueDate.Should().Be(Today.AddDays(3));
        task.CreatedAtUtc.Should().Be(NowUtc);
        task.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetInitialStatusToPending()
    {
        var task = CreateValidTask();

        task.Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        var act = () => CreateValidTask(title: string.Empty);

        act.Should().Throw<DomainException>().WithMessage("*Title is required*");
    }

    [Fact]
    public void Create_WithWhitespaceOnlyTitle_ShouldThrow()
    {
        var act = () => CreateValidTask(title: "   ");

        act.Should().Throw<DomainException>().WithMessage("*Title is required*");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldThrow()
    {
        var tooLong = new string('a', TaskItem.TitleMaxLength + 1);

        var act = () => CreateValidTask(title: tooLong);

        act.Should().Throw<DomainException>().WithMessage($"*{TaskItem.TitleMaxLength}*");
    }

    [Fact]
    public void Create_WithTitleAtMaxLength_ShouldSucceed()
    {
        var maxLength = new string('a', TaskItem.TitleMaxLength);

        var task = CreateValidTask(title: maxLength);

        task.Title.Should().Be(maxLength);
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrow()
    {
        var tooLong = new string('a', TaskItem.DescriptionMaxLength + 1);

        var act = () => CreateValidTask(description: tooLong);

        act.Should().Throw<DomainException>().WithMessage($"*{TaskItem.DescriptionMaxLength}*");
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        var task = CreateValidTask(description: null);

        task.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldThrow()
    {
        var act = () => TaskItem.Create(Guid.Empty, "Title", null, null, NowUtc, Today);

        act.Should().Throw<DomainException>().WithMessage("*Owner id*");
    }

    [Fact]
    public void Create_WithPastDueDate_ShouldThrow()
    {
        var act = () => CreateValidTask(dueDate: Today.AddDays(-1));

        act.Should().Throw<DomainException>().WithMessage("*Due date*");
    }

    [Fact]
    public void Create_WithDueDateOfToday_ShouldSucceed()
    {
        var task = CreateValidTask(dueDate: Today);

        task.DueDate.Should().Be(Today);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldApplyChanges()
    {
        var task = CreateValidTask();
        var updatedAt = NowUtc.AddHours(1);

        task.UpdateDetails("New title", "New description", Today.AddDays(10), updatedAt);

        task.Title.Should().Be("New title");
        task.Description.Should().Be("New description");
        task.DueDate.Should().Be(Today.AddDays(10));
        task.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ShouldThrow()
    {
        var task = CreateValidTask();

        var act = () => task.UpdateDetails(string.Empty, null, null, NowUtc);

        act.Should().Throw<DomainException>().WithMessage("*Title is required*");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndTimestamp()
    {
        var task = CreateValidTask();
        var changedAt = NowUtc.AddDays(1);

        task.ChangeStatus(TaskItemStatus.InProgress, changedAt);

        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.UpdatedAtUtc.Should().Be(changedAt);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_ShouldSucceed()
    {
        var task = CreateValidTask();

        task.ChangeStatus(TaskItemStatus.Completed, NowUtc.AddDays(1));

        task.Status.Should().Be(TaskItemStatus.Completed);
    }

    [Fact]
    public void OwnerId_ShouldRemainImmutable_AfterUpdateAndStatusChange()
    {
        var task = CreateValidTask();

        task.UpdateDetails("Changed", null, null, NowUtc.AddHours(1));
        task.ChangeStatus(TaskItemStatus.Completed, NowUtc.AddHours(2));

        task.OwnerId.Should().Be(OwnerId);
    }
}
