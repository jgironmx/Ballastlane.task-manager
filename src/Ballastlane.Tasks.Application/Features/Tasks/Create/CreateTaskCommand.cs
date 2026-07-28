namespace Ballastlane.Tasks.Application.Features.Tasks.Create;

public sealed record CreateTaskCommand(string Title, string? Description, DateOnly? DueDate);
