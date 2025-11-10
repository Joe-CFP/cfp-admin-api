using AdminApi.Db;
using AdminApi.Lib;

namespace AdminApi.Repositories;

public partial class DatabaseRepository : IDatabaseRepository
{
    private string ConnectionString { get; }

    public DatabaseRepository(ISecretStore secrets)
    {
        Secret db = secrets[SecretName.ProdDatabase];
        ConnectionString = $"Server={db.Endpoint};User={db.Username};Password={db.Password};" +
                           $"Database=login;Convert Zero Datetime = true;Connect Timeout=30;Pooling=true;";
    }

    private static string BuildSelectSql(
        DbTable table,
        IEnumerable<(string SqlExpr, string Alias)>? extras = null)
    {
        IEnumerable<string> fields = table.Fields.Select(f => $"{table.DbName}.{f.DbName} AS {f.Name}");

        if (extras is not null)
            fields = fields.Concat(extras.Select(e => $"{e.SqlExpr} AS {e.Alias}"));

        return $"SELECT {string.Join(", ", fields)} FROM {table.DbName}";
    }
}