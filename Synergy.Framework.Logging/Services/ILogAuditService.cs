namespace Synergy.Framework.Logging.Services;

public interface ILogAuditService
{
    void Add(object newEntity);
    void Update(object newEntity, object oldEntity);
    void SoftDelete(object entity);
    void HardDelete(object entity);
}
