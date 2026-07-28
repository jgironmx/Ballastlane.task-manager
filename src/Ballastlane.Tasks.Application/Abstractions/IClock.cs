namespace Ballastlane.Tasks.Application.Abstractions;

/// <summary>
/// Abstracts the system clock so use cases (and the domain operations they drive) are
/// deterministic and testable. Infrastructure provides the real, UTC-based implementation.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
