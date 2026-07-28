using Ballastlane.Tasks.Application.Abstractions;

namespace Ballastlane.Tasks.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
