using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Application.Contracts;

public sealed record UserDto(Guid Id, string Email, string FirstName, string LastName)
{
    public static UserDto FromIdentityUser(IdentityUserInfo user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName);
}
