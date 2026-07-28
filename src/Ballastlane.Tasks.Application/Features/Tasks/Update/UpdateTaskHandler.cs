using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;
using Ballastlane.Tasks.Domain.Exceptions;

namespace Ballastlane.Tasks.Application.Features.Tasks.Update;

public sealed class UpdateTaskHandler(ITaskRepository taskRepository, ICurrentUser currentUser, IClock clock)
{
    public async Task<Result<TaskDto>> HandleAsync(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure<TaskDto>(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        var task = await taskRepository.GetByIdAsync(command.TaskId, ownerId, cancellationToken);
        if (task is null)
        {
            return Result.Failure<TaskDto>(UseCaseError.NotFound("task.not_found", "Task not found."));
        }

        try
        {
            task.UpdateDetails(command.Title, command.Description, command.DueDate, clock.UtcNow);
            await taskRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(TaskDto.FromDomain(task));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TaskDto>(UseCaseError.Validation("task.invalid", ex.Message));
        }
    }
}
