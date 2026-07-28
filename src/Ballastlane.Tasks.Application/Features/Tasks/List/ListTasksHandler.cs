using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Tasks.List;

public sealed class ListTasksHandler(ITaskRepository taskRepository, ICurrentUser currentUser)
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<TaskDto>>> HandleAsync(ListTasksQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure<PagedResult<TaskDto>>(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        if (query.Page < 1)
        {
            return Result.Failure<PagedResult<TaskDto>>(
                UseCaseError.Validation("tasks.invalid_page", "page must be 1 or greater."));
        }

        if (query.PageSize is < 1 or > MaxPageSize)
        {
            return Result.Failure<PagedResult<TaskDto>>(
                UseCaseError.Validation("tasks.invalid_page_size", $"pageSize must be between 1 and {MaxPageSize}."));
        }

        var searchText = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim();

        var tasks = await taskRepository.ListAsync(ownerId, query.Status, searchText, query.Page, query.PageSize, cancellationToken);
        var totalCount = await taskRepository.CountAsync(ownerId, query.Status, searchText, cancellationToken);

        var items = tasks.Select(TaskDto.FromDomain).ToArray();
        return Result.Success(new PagedResult<TaskDto>(items, query.Page, query.PageSize, totalCount));
    }
}
