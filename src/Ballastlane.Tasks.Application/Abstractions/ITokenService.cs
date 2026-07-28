namespace Ballastlane.Tasks.Application.Abstractions;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAtUtc);

/// <summary>
/// Port over JWT issuance. Infrastructure implements this against the configured signing key,
/// issuer, and audience; Application only ever deals with the resulting opaque token value and
/// its expiration.
/// </summary>
public interface ITokenService
{
    AccessToken CreateToken(IdentityUserInfo user);
}
