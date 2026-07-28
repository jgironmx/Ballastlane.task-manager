using System.Text.RegularExpressions;

namespace Ballastlane.Tasks.Application.Common;

/// <summary>Use-case-level email format validation (see docs/decisions/ADR-005 validation strategy).</summary>
public static partial class EmailValidator
{
    public static bool IsValid(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
