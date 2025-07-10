namespace Synergy.Framework.EfCore.Configuration;

public class EfCoreModuleOptions
{
    /// <summary>Default connection string name for BOTH read & write when <see cref="SeparateReadDatabase"/> is false.</summary>
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>Set true to use a dedicated read‑only DB.</summary>
    public bool SeparateReadDatabase { get; set; } = false;

    /// <summary>If <see cref="SeparateReadDatabase"/> is true this is used for read queries.</summary>
    public string ReadConnectionStringName { get; set; } = "ReadConnection";

    /// <summary>If <see cref="SeparateReadDatabase"/> is true this is used for commands.</summary>
    public string WriteConnectionStringName { get; set; } = "WriteConnection";

    /// <summary>Auto‑apply migrations for write context.</summary>
    public bool AutoMigrate { get; set; } = true;
}
