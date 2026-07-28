using Ballastlane.Tasks.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ballastlane.Tasks.Infrastructure.IntegrationTests.Persistence;

[Trait("Category", "Integration")]
[Collection(InfrastructureTestGroup.Name)]
public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task Database_ShouldBeReachable_AfterMigration()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();

        var canConnect = await dbContext.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_ShouldHaveNoPendingMigrations()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();

        var pending = await dbContext.Database.GetPendingMigrationsAsync();

        pending.Should().BeEmpty();
    }

    [Fact]
    public async Task TasksTable_ShouldExist_AndBeQueryable()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();

        // Throws if the "Tasks" table (created by the InitialCreate migration) does not exist.
        var act = () => dbContext.Tasks.CountAsync();

        await act.Should().NotThrowAsync();
    }
}
