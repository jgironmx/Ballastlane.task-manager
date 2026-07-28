namespace Ballastlane.Tasks.Api.Contracts.Tasks;

public sealed record UpdateTaskRequest(string Title, string? Description, DateOnly? DueDate);
