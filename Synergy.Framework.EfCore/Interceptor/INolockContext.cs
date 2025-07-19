namespace Synergy.Framework.EfCore.Interceptor;

public interface INolockContext
{
    bool UseNoLock { get; set; }
}
