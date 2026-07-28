using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Api.Contracts.Tasks;

public sealed record ChangeTaskStatusRequest(TaskItemStatus Status);
