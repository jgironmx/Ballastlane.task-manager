using Ballastlane.Tasks.Infrastructure.Identity;
using Ballastlane.Tasks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ballastlane.Tasks.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// Applies migrations against a real SQL Server LocalDB database once per test class and drops it
/// afterwards. Docker was not available in the environment this sprint was built in (see
/// docs/decisions/ADR-005-testing-strategy.md), so LocalDB is used as the documented fallback
/// instead of SQL Server Testcontainers — never EF Core InMemory, which would not exercise real
/// SQL Server constraint/index/query behavior.
/// </summary>
public sealed class InfrastructureDatabaseFixture : IAsyncLifetime
{
    public const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BallastlaneTasksDb_InfrastructureTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    public static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>A minimal DI container mirroring the API's Identity registration, for tests that
    /// need a real <c>UserManager&lt;ApplicationUser&gt;</c> without booting the full host.</summary>
    public static ServiceProvider BuildIdentityServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider();
    }
}
