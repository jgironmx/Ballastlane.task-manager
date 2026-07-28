using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Domain.Tasks;
using Ballastlane.Tasks.Infrastructure.Identity;
using Ballastlane.Tasks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ballastlane.Tasks.Infrastructure.Seed;

/// <summary>
/// Idempotent development-only seed data: one demo user and four demo tasks covering each status
/// plus a future due date. Only invoked from <c>Program.cs</c> when
/// <c>IHostEnvironment.IsDevelopment()</c> is true.
/// </summary>
public static class DevelopmentDataSeeder
{
    public const string DemoUserEmail = "demo@ballastlane.local";
    public const string DemoUserPassword = "Demo1234!";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var clock = services.GetRequiredService<IClock>();

        var demoUser = await userManager.FindByEmailAsync(DemoUserEmail);
        if (demoUser is null)
        {
            demoUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = DemoUserEmail,
                Email = DemoUserEmail,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "User",
                CreatedAtUtc = clock.UtcNow,
            };

            var creationResult = await userManager.CreateAsync(demoUser, DemoUserPassword);
            if (!creationResult.Succeeded)
            {
                var errors = string.Join(", ", creationResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed the demo user: {errors}");
            }
        }

        var hasSeededTasks = await dbContext.Tasks.AnyAsync(t => t.OwnerId == demoUser.Id, cancellationToken);
        if (hasSeededTasks)
        {
            return;
        }

        var now = clock.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var pending = TaskItem.Create(
            demoUser.Id,
            "Plan sprint backlog",
            "Groom and prioritize next sprint's tickets.",
            today.AddDays(3),
            now,
            today);

        var inProgress = TaskItem.Create(
            demoUser.Id,
            "Write architecture documentation",
            "Document the Clean Architecture layering and dependency rules.",
            today.AddDays(1),
            now,
            today);
        inProgress.ChangeStatus(TaskItemStatus.InProgress, now);

        var completed = TaskItem.Create(
            demoUser.Id,
            "Set up repository baseline",
            "Solution structure, CI, and architecture decision records.",
            null,
            now,
            today);
        completed.ChangeStatus(TaskItemStatus.InProgress, now);
        completed.ChangeStatus(TaskItemStatus.Completed, now);

        var futureDueDate = TaskItem.Create(
            demoUser.Id,
            "Prepare a demo",
            "Rehearse the end-to-end walkthrough.",
            today.AddDays(30),
            now,
            today);

        dbContext.Tasks.AddRange(pending, inProgress, completed, futureDueDate);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
