using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Application.Features.Tasks.ChangeStatus;

public sealed record ChangeTaskStatusCommand(Guid TaskId, TaskItemStatus Status);
