using Synergy.Framework.Logging.Services;
using Synergy.Framework.Shared.Abstractions;

namespace Synergy.Framework.Logging.Handlers;

public class DefaultAuditLogHandler: IAuditLogHandler
{
    private readonly ILogAuditService _logAuditService;

    public DefaultAuditLogHandler(ILogAuditService logAuditService)
    {
        _logAuditService = logAuditService;
    }

    public void Add(object newEntity)
    {
        _logAuditService.Add(newEntity);
    }

    public void Update(object newEntity, object oldEntity)
    {
        _logAuditService.Update(newEntity, oldEntity);
    }

    public void SoftDelete(object entity)
    {
        _logAuditService.SoftDelete(entity);
    }

    public void HardDelete(object entity)
    {
        _logAuditService.HardDelete(entity);
    }
}
