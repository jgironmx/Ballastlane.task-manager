using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Infrastructure.Authentication;
using Ballastlane.Tasks.Infrastructure.Clock;
using Ballastlane.Tasks.Infrastructure.Identity;
using Ballastlane.Tasks.Infrastructure.Persistence;
using Ballastlane.Tasks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ballastlane.Tasks.Infrastructure;

/// <summary>Composition helper for Infrastructure's own services. Called from the API composition root.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. See README.md for LocalDB / user-secrets setup.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "Jwt:SigningKey must be configured and at least 32 characters (256 bits) for HMAC-SHA256.")
            .ValidateOnStart();

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
