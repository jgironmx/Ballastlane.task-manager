using Ballastlane.Tasks.Api.Contracts.Tasks;
using Ballastlane.Tasks.Api.ErrorHandling;
using Ballastlane.Tasks.Application.Features.Tasks.ChangeStatus;
using Ballastlane.Tasks.Application.Features.Tasks.Create;
using Ballastlane.Tasks.Application.Features.Tasks.Delete;
using Ballastlane.Tasks.Application.Features.Tasks.GetById;
using Ballastlane.Tasks.Application.Features.Tasks.List;
using Ballastlane.Tasks.Application.Features.Tasks.Update;
using Ballastlane.Tasks.Domain.Tasks;

namespace Ballastlane.Tasks.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapGet(string.Empty, async (
                ListTasksHandler handler,
                TaskItemStatus? status,
                string? search,
                CancellationToken cancellationToken,
                int page = 1,
                int pageSize = 20) =>
            {
                var query = new ListTasksQuery(status, search, page, pageSize);
                var result = await handler.HandleAsync(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
            })
            .WithName("ListTasks");

        group.MapGet("/{id:guid}", async (Guid id, GetTaskByIdHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetTaskByIdQuery(id), cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
            })
            .WithName("GetTaskById");

        group.MapPost(string.Empty, async (CreateTaskRequest request, CreateTaskHandler handler, CancellationToken cancellationToken) =>
            {
                var command = new CreateTaskCommand(request.Title, request.Description, request.DueDate);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/tasks/{result.Value.Id}", result.Value)
                    : result.Error.ToProblem();
            })
            .WithName("CreateTask");

        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, UpdateTaskHandler handler, CancellationToken cancellationToken) =>
            {
                var command = new UpdateTaskCommand(id, request.Title, request.Description, request.DueDate);
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
            })
            .WithName("UpdateTask");

        group.MapPatch("/{id:guid}/status", async (
                Guid id,
                ChangeTaskStatusRequest request,
                ChangeTaskStatusHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new ChangeTaskStatusCommand(id, request.Status);
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
            })
            .WithName("ChangeTaskStatus");

        group.MapDelete("/{id:guid}", async (Guid id, DeleteTaskHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new DeleteTaskCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
            })
            .WithName("DeleteTask");
    }
}
