using Ballastlane.Tasks.Domain.Tasks;
using Ballastlane.Tasks.Infrastructure.Identity;
using Ballastlane.Tasks.Infrastructure.IntegrationTests.Fixtures;
using Ballastlane.Tasks.Infrastructure.Persistence;
using Ballastlane.Tasks.Infrastructure.Persistence.Repositories;

namespace Ballastlane.Tasks.Infrastructure.IntegrationTests.Persistence;

[Trait("Category", "Integration")]
[Collection(InfrastructureTestGroup.Name)]
public sealed class TaskRepositoryTests
{
    private static readonly DateTimeOffset NowUtc = DateTimeOffset.UtcNow;
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.UtcDateTime);

    /// <summary>Tasks.OwnerId has a foreign key to AspNetUsers, so every test needs a real user row.</summary>
    private static async Task<Guid> CreateOwnerAsync(ApplicationDbContext dbContext)
    {
        var id = Guid.NewGuid();
        var email = $"{id}@example.com";
        dbContext.Users.Add(new ApplicationUser
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = "Test",
            LastName = "Owner",
            CreatedAtUtc = NowUtc,
        });
        await dbContext.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldReturnPersistedTask()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var ownerId = await CreateOwnerAsync(dbContext);
        var repository = new TaskRepository(dbContext);
        var task = TaskItem.Create(ownerId, "Persisted task", "Description", null, NowUtc, Today);

        repository.Add(task);
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var readDbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var readRepository = new TaskRepository(readDbContext);
        var persisted = await readRepository.GetByIdAsync(task.Id, ownerId, CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be("Persisted task");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskBelongsToAnotherOwner_ShouldReturnNull()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var ownerId = await CreateOwnerAsync(dbContext);
        var otherOwnerId = await CreateOwnerAsync(dbContext);
        var repository = new TaskRepository(dbContext);
        var task = TaskItem.Create(ownerId, "Owner-only task", null, null, NowUtc, Today);
        repository.Add(task);
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var readDbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var readRepository = new TaskRepository(readDbContext);
        var result = await readRepository.GetByIdAsync(task.Id, otherOwnerId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ShouldOnlyReturnTasksForTheGivenOwner()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var ownerId = await CreateOwnerAsync(dbContext);
        var otherOwnerId = await CreateOwnerAsync(dbContext);
        var repository = new TaskRepository(dbContext);
        repository.Add(TaskItem.Create(ownerId, "Mine", null, null, NowUtc, Today));
        repository.Add(TaskItem.Create(otherOwnerId, "Not mine", null, null, NowUtc, Today));
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var readDbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var readRepository = new TaskRepository(readDbContext);
        var results = await readRepository.ListAsync(ownerId, status: null, searchText: null, page: 1, pageSize: 20, CancellationToken.None);

        results.Should().ContainSingle(t => t.Title == "Mine");
    }

    [Fact]
    public async Task UpdateDetails_ThenSaveChanges_ShouldPersistChanges()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var ownerId = await CreateOwnerAsync(dbContext);
        var repository = new TaskRepository(dbContext);
        var task = TaskItem.Create(ownerId, "Original", null, null, NowUtc, Today);
        repository.Add(task);
        await repository.SaveChangesAsync(CancellationToken.None);

        task.UpdateDetails("Updated title", "Updated description", Today.AddDays(2), NowUtc.AddMinutes(5));
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var readDbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var readRepository = new TaskRepository(readDbContext);
        var persisted = await readRepository.GetByIdAsync(task.Id, ownerId, CancellationToken.None);

        persisted!.Title.Should().Be("Updated title");
        persisted.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task Remove_ThenSaveChanges_ShouldDeleteTask()
    {
        await using var dbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var ownerId = await CreateOwnerAsync(dbContext);
        var repository = new TaskRepository(dbContext);
        var task = TaskItem.Create(ownerId, "To delete", null, null, NowUtc, Today);
        repository.Add(task);
        await repository.SaveChangesAsync(CancellationToken.None);

        repository.Remove(task);
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var readDbContext = InfrastructureDatabaseFixture.CreateDbContext();
        var readRepository = new TaskRepository(readDbContext);
        var result = await readRepository.GetByIdAsync(task.Id, ownerId, CancellationToken.None);

        result.Should().BeNull();
    }
}
