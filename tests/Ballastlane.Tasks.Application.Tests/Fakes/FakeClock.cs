using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Application.Tests.Fakes;

public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}
