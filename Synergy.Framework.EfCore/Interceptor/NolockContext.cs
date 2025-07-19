namespace Synergy.Framework.EfCore.Interceptor;

internal class NolockContext: INolockContext
{
    public bool UseNoLock { get; set; } = true;
}
