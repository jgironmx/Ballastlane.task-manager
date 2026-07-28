namespace Ballastlane.Tasks.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    /// <summary>
    /// Never committed with a real value — see appsettings.Development.json and the user-secrets
    /// instructions in the root README. Must be at least 32 bytes (256 bits) for HMAC-SHA256.
    /// </summary>
    public required string SigningKey { get; set; }

    public int ExpirationMinutes { get; set; } = 60;
}
