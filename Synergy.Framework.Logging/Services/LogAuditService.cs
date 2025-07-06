using Serilog;
using Synergy.Framework.Logging.Enums;
using Synergy.Framework.Logging.Models;
using Synergy.Framework.Shared.Exceptions;
using Synergy.Framework.Shared.Options;
using System.Reflection;
using System.Text.Json;

namespace Synergy.Framework.Logging.Services;

internal class LogAuditService : ILogAuditService
{
    private readonly Serilog.ILogger _logger;

    public LogAuditService()
    {
        _logger = Log.Logger;
    }

    public void Add(object newEntity)
    {
        if (newEntity is null)
            throw new ArgumentNullException(nameof(newEntity));

        var entityType = newEntity.GetType().Name;
        var description = $"{entityType} added.";

        var auditLog = new AuditLogDto
        {
            EntityType = entityType,
            ActionType = nameof(ActionType.Add),
            Changes = newEntity,
            Description = description
        };

        LogAddInformation(description, auditLog);
    }

    public void Update(object newEntity, object oldEntity)
    {
        if (newEntity is null)
            throw new ArgumentNullException(nameof(newEntity));

        if (oldEntity is null)
            throw new ArgumentNullException(nameof(oldEntity));

        if (newEntity.GetType() != oldEntity.GetType())
            throw new SynergyException("newEntity and oldEntity must be of the same type.","AUDIT_UPDATE_ARGUMENT");

        var entityType = newEntity.GetType().Name;
        var message = $"{entityType} updated.";
        object changes;

        var oldProps = oldEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var newProps = newEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var differences = new Dictionary<string, object>();

        // ID bilgisi ekleniyor
        var idProp = newProps.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        if (idProp != null)
        {
            var idVal = idProp.GetValue(newEntity);
            if (idVal != null) // null kontrolü eklendi
            {
                differences["Id"] = idVal;
            }
        }

        foreach (var prop in newProps)
        {
            if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue; // ID zaten eklendi

            var oldProp = oldProps.FirstOrDefault(p => p.Name == prop.Name);
            if (oldProp == null)
                continue;

            var oldVal = oldProp.GetValue(oldEntity)?.ToString() ?? string.Empty;
            var newVal = prop.GetValue(newEntity)?.ToString() ?? string.Empty;

            if (oldVal != newVal)
            {
                differences[prop.Name] = new { Old = oldVal, New = newVal };
            }
        }

        changes = differences;
        var description = $"{entityType} table updated. {differences.Count} field(s) changed.";

        var auditLog = new AuditLogDto
        {
            EntityType = entityType,
            ActionType = nameof(ActionType.Update),
            Changes = changes,
            Description = description
        };

        LogAddInformation(message, auditLog);
    }

    public void SoftDelete(object entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType().Name;
        var description = $"{entityType} soft deleted.";

        var auditLog = new AuditLogDto
        {
            EntityType = entityType,
            ActionType = nameof(ActionType.SoftDelete),
            Changes = entity,
            Description = description
        };

        LogAddInformation(description, auditLog);
    }

    public void HardDelete(object entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType().Name;
        var description = $"{entityType} hard deleted.";

        var auditLog = new AuditLogDto
        {
            EntityType = entityType,
            ActionType = nameof(ActionType.HardDelete),
            Changes = entity,
            Description = description
        };

        LogAddInformation(description, auditLog);
    }

    private void LogAddInformation(string message, AuditLogDto auditLogDto)
    {
        _logger
            .ForContext("LogType", nameof(LogType.Audit))
            .Information("{Type} | {Message} | {@PayloadJson}",
            nameof(LogType.Audit), message, JsonSerializer.Serialize(auditLogDto, SynergyJsonSerializerOptions.Cached));
    }
}
