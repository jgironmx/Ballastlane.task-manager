using Ballastlane.Tasks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API host against a dedicated SQL Server LocalDB test database (Docker was not
/// available in the environment this sprint was built in — see
/// docs/decisions/ADR-005-testing-strategy.md) with a fixed, test-only JWT signing key. Migrations
/// and demo-data seeding run exactly as they would in Development, via <c>Program.cs</c>'s own
/// startup logic — nothing is duplicated here.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BallastlaneTasksDb_ApiIntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public const string SigningKey = "api-integration-tests-only-signing-key-never-use-in-prod-32+chars";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Issuer"] = "Ballastlane.Tasks.Tests",
                ["Jwt:Audience"] = "Ballastlane.Tasks.Tests.Client",
                ["Jwt:SigningKey"] = SigningKey,
                ["Jwt:ExpirationMinutes"] = "60",
            });
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
