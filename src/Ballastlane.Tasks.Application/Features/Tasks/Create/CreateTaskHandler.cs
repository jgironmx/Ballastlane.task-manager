using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Common;
using Ballastlane.Tasks.Application.Contracts;
using Ballastlane.Tasks.Domain.Exceptions;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Features.Tasks.Create;

public sealed class CreateTaskHandler(ITaskRepository taskRepository, ICurrentUser currentUser, IClock clock)
{
    public async Task<Result<TaskDto>> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure<TaskDto>(UseCaseError.Unauthorized("auth.required", "Authentication is required."));
        }

        try
        {
            var now = clock.UtcNow;
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            var task = TaskItem.Create(ownerId, command.Title, command.Description, command.DueDate, now, today);

            taskRepository.Add(task);
            await taskRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(TaskDto.FromDomain(task));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TaskDto>(UseCaseError.Validation("task.invalid", ex.Message));
        }
    }
}
