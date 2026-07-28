using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ballastlane.Tasks.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ballastlane.Tasks.Infrastructure.Authentication;

public sealed class TokenService(IOptions<JwtOptions> jwtOptions, IClock clock) : ITokenService
{
    public AccessToken CreateToken(IdentityUserInfo user)
    {
        var options = jwtOptions.Value;
        var expiresAtUtc = clock.UtcNow.AddMinutes(options.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(value, expiresAtUtc);
    }
}
