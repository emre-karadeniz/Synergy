using System.Text.Json;

namespace Synergy.Framework.Logging.Options;

internal class LoggingJsonSerializerOptions
{
    internal static readonly JsonSerializerOptions Cached = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // JSON çıktısı camelCase olur
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Escape karakterlerini kaldırır
        WriteIndented = true, // JSON'un minify edilmiş olarak kaydedilmesini sağlar     
    };
}
