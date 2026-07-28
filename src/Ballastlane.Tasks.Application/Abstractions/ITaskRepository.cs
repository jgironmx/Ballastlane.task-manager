using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Abstractions;

/// <summary>
/// Persistence port for <see cref="TaskItem"/>. Every read is scoped to a specific owner at the
/// query level (never "load everything, filter in memory") so cross-user data never leaves the
/// database. This is intentionally not a generic repository — task-specific operations only.
/// </summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid taskId, Guid ownerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskItem>> ListAsync(
        Guid ownerId,
        TaskItemStatus? status,
        string? searchText,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAsync(Guid ownerId, TaskItemStatus? status, string? searchText, CancellationToken cancellationToken);

    void Add(TaskItem task);

    void Remove(TaskItem task);

    /// <summary>
    /// Persists pending changes. The underlying <c>DbContext</c> already represents the
    /// transaction boundary for the single-aggregate operations in this exercise, so a separate
    /// <c>IUnitOfWork</c> abstraction is intentionally omitted — see
    /// docs/decisions/ADR-007-application-abstractions.md.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
