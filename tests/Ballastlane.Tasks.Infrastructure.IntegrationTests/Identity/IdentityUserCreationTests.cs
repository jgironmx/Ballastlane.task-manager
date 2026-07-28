using Ballastlane.Tasks.Infrastructure.Identity;
using Ballastlane.Tasks.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Ballastlane.Tasks.Infrastructure.IntegrationTests.Identity;

[Trait("Category", "Integration")]
[Collection(InfrastructureTestGroup.Name)]
public sealed class IdentityUserCreationTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_ShouldPersistUser()
    {
        await using var provider = InfrastructureDatabaseFixture.BuildIdentityServiceProvider();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = $"{Guid.NewGuid()}@example.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(user, "Password1!");

        result.Succeeded.Should().BeTrue();

        var persisted = await userManager.FindByEmailAsync(email);
        persisted.Should().NotBeNull();
        persisted!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldFail()
    {
        await using var provider = InfrastructureDatabaseFixture.BuildIdentityServiceProvider();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = $"{Guid.NewGuid()}@example.com";
        var first = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        (await userManager.CreateAsync(first, "Password1!")).Succeeded.Should().BeTrue();

        var second = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Jane2",
            LastName = "Doe2",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(second, "Password2!");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail");
    }
}
