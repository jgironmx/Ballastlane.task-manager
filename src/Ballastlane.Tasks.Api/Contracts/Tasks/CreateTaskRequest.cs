namespace Ballastlane.Tasks.Api.Contracts.Tasks;

public sealed record CreateTaskRequest(string Title, string? Description, DateOnly? DueDate);
