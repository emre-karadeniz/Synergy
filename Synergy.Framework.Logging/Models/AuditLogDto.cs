namespace Synergy.Framework.Logging.Models;

internal class AuditLogDto
{
    public string EntityType { get; set; } = null!;
    public string ActionType { get; set; } = null!;
    public object Changes { get; set; } = null!;
    public string Description { get; set; } = null!;
}
