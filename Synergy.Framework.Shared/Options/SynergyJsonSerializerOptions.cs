using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Synergy.Framework.Shared.Options;

public static class SynergyJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Cached = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = false
    };
}
