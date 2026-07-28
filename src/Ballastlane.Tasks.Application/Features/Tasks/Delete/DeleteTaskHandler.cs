using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;

namespace Ballastlane.Tasks.Application.Features.Tasks.Delete;

public sealed class DeleteTaskHandler(ITaskRepository taskRepository, ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        var task = await taskRepository.GetByIdAsync(command.TaskId, ownerId, cancellationToken);
        if (task is null)
        {
            return Result.Failure(UseCaseError.NotFound("task.not_found", "Task not found."));
        }

        taskRepository.Remove(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
