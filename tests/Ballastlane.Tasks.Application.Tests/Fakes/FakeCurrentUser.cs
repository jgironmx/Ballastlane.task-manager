using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Application.Tests.Fakes;

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => UserId.HasValue;

    public Guid? UserId { get; set; }

    public static FakeCurrentUser Anonymous() => new();

    public static FakeCurrentUser For(Guid userId) => new() { UserId = userId };
}
