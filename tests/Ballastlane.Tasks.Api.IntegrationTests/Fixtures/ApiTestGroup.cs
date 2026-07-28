namespace Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

/// <summary>
/// All API integration test classes join this collection so xUnit runs them sequentially against
/// one <see cref="CustomWebApplicationFactory"/>/database instance, instead of racing multiple
/// classes' factories to create/drop the same LocalDB database in parallel.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Api Integration Tests";
}
