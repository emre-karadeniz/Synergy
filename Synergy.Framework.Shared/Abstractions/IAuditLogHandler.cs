namespace Synergy.Framework.Shared.Abstractions;

public interface IAuditLogHandler
{
    void Add(object newEntity);
    void Update(object newEntity, object oldEntity);
    void SoftDelete(object entity);
    void HardDelete(object entity);
}
