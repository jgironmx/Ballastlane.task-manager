using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;

namespace Ballastlane.Tasks.Application.Features.Tasks.GetById;

public sealed class GetTaskByIdHandler(ITaskRepository taskRepository, ICurrentUser currentUser)
{
    public async Task<Result<TaskDto>> HandleAsync(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure<TaskDto>(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        var task = await taskRepository.GetByIdAsync(query.TaskId, ownerId, cancellationToken);
        if (task is null)
        {
            // Also returned when the task belongs to another user — see the cross-user 404 decision
            // in docs/decisions/ADR-008-cross-user-404.md.
            return Result.Failure<TaskDto>(UseCaseError.NotFound("task.not_found", "Task not found."));
        }

        return Result.Success(TaskDto.FromDomain(task));
    }
}
