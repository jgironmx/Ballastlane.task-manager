namespace Ballastlane.Tasks.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// All integration test classes that touch the shared LocalDB test database join this collection
/// so xUnit runs them sequentially against one <see cref="InfrastructureDatabaseFixture"/> instance,
/// instead of racing multiple classes' fixtures to create/drop the same database in parallel.
/// </summary>
[CollectionDefinition(Name)]
public sealed class InfrastructureTestGroup : ICollectionFixture<InfrastructureDatabaseFixture>
{
    public const string Name = "Infrastructure Database";
}
