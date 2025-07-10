namespace Synergy.Framework.Dapper.Configuration;

public class DapperModuleOptions
{
    public bool SeparateReadDatabase { get; set; } = false;

    public string ConnectionStringName { get; set; } = "DefaultConnection";
    public string ReadConnectionStringName { get; set; } = "ReadConnection";
    public string WriteConnectionStringName { get; set; } = "WriteConnection";

    public bool EnableSoftDeleteFilterByDefault { get; set; } = true;
}
