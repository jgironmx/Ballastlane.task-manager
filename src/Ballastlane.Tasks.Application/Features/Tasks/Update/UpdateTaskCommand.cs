namespace Ballastlane.Tasks.Application.Features.Tasks.Update;

public sealed record UpdateTaskCommand(Guid TaskId, string Title, string? Description, DateOnly? DueDate);
