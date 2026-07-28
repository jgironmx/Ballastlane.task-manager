namespace Ballastlane.Tasks.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")))
            .WithName("GetHealth")
            .WithTags("Health")
            .AllowAnonymous();
    }
}

internal sealed record HealthResponse(string Status);
