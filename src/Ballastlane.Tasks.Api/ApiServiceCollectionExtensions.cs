using System.Text;
using Ballastlane.Tasks.Api.Authentication;
using Ballastlane.Tasks.Api.ErrorHandling;
using Ballastlane.Tasks.Application.Abstractions;
using Ballastlane.Tasks.Application.Features.Authentication.GetCurrentUser;
using Ballastlane.Tasks.Application.Features.Authentication.Login;
using Ballastlane.Tasks.Application.Features.Authentication.Register;
using Ballastlane.Tasks.Application.Features.Tasks.ChangeStatus;
using Ballastlane.Tasks.Application.Features.Tasks.Create;
using Ballastlane.Tasks.Application.Features.Tasks.Delete;
using Ballastlane.Tasks.Application.Features.Tasks.GetById;
using Ballastlane.Tasks.Application.Features.Tasks.List;
using Ballastlane.Tasks.Application.Features.Tasks.Update;
using Ballastlane.Tasks.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Ballastlane.Tasks.Api;

public static class ApiServiceCollectionExtensions
{
    public const string DevelopmentCorsPolicyName = "AngularDev";

    /// <summary>
    /// Development-only CORS for the Angular dev server. Never registered or applied outside
    /// Development (see Program.cs) — no permissive production-wide policy exists. The allowed
    /// origin list is configurable via <c>Cors:AllowedOrigins</c>, defaulting to the Angular CLI's
    /// default dev-server origin. No <c>AllowAnyOrigin</c>, and no credentials mode is requested
    /// (the SPA authenticates via an <c>Authorization</c> header, not cookies, so
    /// <c>AllowCredentials</c> is unnecessary).
    /// </summary>
    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentCorsPolicyName, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }

    public static IServiceCollection AddApiComposition(this IServiceCollection services)
    {
        // Use-case handlers — thin, stateless, request-scoped.
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<GetTaskByIdHandler>();
        services.AddScoped<ListTasksHandler>();
        services.AddScoped<UpdateTaskHandler>();
        services.AddScoped<ChangeTaskStatusHandler>();
        services.AddScoped<DeleteTaskHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                // Without this, a missing/invalid/expired bearer token falls through to
                // JwtBearerHandler's own default 401 response, which has a different body shape
                // than every other 401 in this API (UseCaseError.Unauthorized -> ToProblem()).
                // Routing it through the same Problem Details shape here keeps the error contract
                // consistent regardless of whether the 401 came from authentication or a use case.
                bearerOptions.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Authentication is required.",
                            Type = "urn:ballastlane-tasks:error:auth.required",
                        };

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddOpenApiWithBearerAuth(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                var components = document.Components ??= new OpenApiComponents();
                var securitySchemes = components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                securitySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter the JWT returned by POST /api/auth/login.",
                };

                var bearerReference = new OpenApiSecuritySchemeReference("Bearer", document);
                var security = document.Security ??= [];
                security.Add(new OpenApiSecurityRequirement
                {
                    [bearerReference] = [],
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
