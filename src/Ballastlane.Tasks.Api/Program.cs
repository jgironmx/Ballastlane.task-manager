using System.Text.Json.Serialization;
using Ballastlane.Tasks.Api;
using Ballastlane.Tasks.Api.Endpoints;
using Ballastlane.Tasks.Infrastructure;
using Ballastlane.Tasks.Infrastructure.Persistence;
using Ballastlane.Tasks.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiComposition();
builder.Services.AddJwtAuthentication();
builder.Services.AddOpenApiWithBearerAuth();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDevelopmentCors(builder.Configuration);
}

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors(ApiServiceCollectionExtensions.DevelopmentCorsPolicyName);

    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapTaskEndpoints();

app.Run();

/// <summary>
/// Exposes the implicit top-level Program class so <c>WebApplicationFactory&lt;Program&gt;</c>
/// can be used from the API integration test project.
/// </summary>
public partial class Program;
