using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Features.Tasks.List;

public sealed record ListTasksQuery(TaskItemStatus? Status = null, string? SearchText = null, int Page = 1, int PageSize = 20);
