using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Tests.Fakes;

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = [];

    public IReadOnlyList<TaskItem> Tasks => _tasks;

    public Task<TaskItem?> GetByIdAsync(Guid taskId, Guid ownerId, CancellationToken cancellationToken) =>
        Task.FromResult(_tasks.SingleOrDefault(t => t.Id == taskId && t.OwnerId == ownerId));

    public Task<IReadOnlyList<TaskItem>> ListAsync(
        Guid ownerId,
        TaskItemStatus? status,
        string? searchText,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var results = Filter(ownerId, status, searchText)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<TaskItem>>(results);
    }

    public Task<int> CountAsync(Guid ownerId, TaskItemStatus? status, string? searchText, CancellationToken cancellationToken) =>
        Task.FromResult(Filter(ownerId, status, searchText).Count());

    public void Add(TaskItem task) => _tasks.Add(task);

    public void Remove(TaskItem task) => _tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private IEnumerable<TaskItem> Filter(Guid ownerId, TaskItemStatus? status, string? searchText) =>
        _tasks.Where(t => t.OwnerId == ownerId)
            .Where(t => status is null || t.Status == status)
            .Where(t => searchText is null || t.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
}
