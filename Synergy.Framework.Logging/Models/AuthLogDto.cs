namespace Synergy.Framework.Logging.Models;

internal class AuthLogDto
{
    public bool IsSuccessful { get; set; }
    public string Explanation { get; set; } = null!;
}
