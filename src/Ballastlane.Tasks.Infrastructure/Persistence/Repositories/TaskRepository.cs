using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ballastlane.Tasks.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid taskId, Guid ownerId, CancellationToken cancellationToken) =>
        dbContext.Tasks.SingleOrDefaultAsync(t => t.Id == taskId && t.OwnerId == ownerId, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> ListAsync(
        Guid ownerId,
        TaskItemStatus? status,
        string? searchText,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        await Filter(ownerId, status, searchText)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid ownerId, TaskItemStatus? status, string? searchText, CancellationToken cancellationToken) =>
        Filter(ownerId, status, searchText).CountAsync(cancellationToken);

    public void Add(TaskItem task) => dbContext.Tasks.Add(task);

    public void Remove(TaskItem task) => dbContext.Tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<TaskItem> Filter(Guid ownerId, TaskItemStatus? status, string? searchText)
    {
        var query = dbContext.Tasks.Where(t => t.OwnerId == ownerId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(t => EF.Functions.Like(t.Title, $"%{searchText}%"));
        }

        return query;
    }
}
