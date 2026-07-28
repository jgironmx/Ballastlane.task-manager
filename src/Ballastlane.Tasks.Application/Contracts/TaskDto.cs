using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Contracts;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateOnly? DueDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static TaskDto FromDomain(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.DueDate, task.CreatedAtUtc, task.UpdatedAtUtc);
}
