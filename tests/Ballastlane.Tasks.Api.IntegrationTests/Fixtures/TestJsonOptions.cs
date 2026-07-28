using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ballastlane.Tasks.Api.IntegrationTests.Fixtures;

/// <summary>Mirrors the server's <c>ConfigureHttpJsonOptions</c> enum-as-string setup (see
/// Program.cs) so response bodies containing <c>TaskItemStatus</c> deserialize correctly in tests.</summary>
internal static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
