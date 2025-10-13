using System.Diagnostics;
using AdminApi.Db;
using AdminApi.Entities;
using AdminApi.Lib;

namespace AdminApi.Repositories;

using Dapper;
using MySql.Data.MySqlClient;

public interface IDatabaseRepository
{
    Task<IEnumerable<Organisation>> GetAllOrganisationsAsync();
    Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationsAsync(string query, int limit = 10);
    Task<Member?> GetMemberByIdAsync(int id);
    Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(string query, int limit = 10);
}

public class DatabaseRepository : IDatabaseRepository
{
    private string ConnectionString { get; }

    public DatabaseRepository(ISecretStore secrets)
    {
        Secret db = secrets[SecretName.ProdDatabase];
        ConnectionString = $"Server={db.Endpoint};User={db.Username};Password={db.Password};" +
                           $"Database=login;Convert Zero Datetime = true;Connect Timeout=30;Pooling=true;";
    }

    public async Task<IEnumerable<Organisation>> GetAllOrganisationsAsync()
    {
        await using MySqlConnection connection = new(ConnectionString);

        Stopwatch stopwatch = Stopwatch.StartNew();

        string orgSql = BuildSelectSql(TableDefinitions.OrganisationTable);
        List<Organisation> organisations = (await connection.QueryAsync<Organisation>(orgSql)).ToList();

        string memberSql = BuildSelectSql(TableDefinitions.MemberTable) + " WHERE orgguid IS NOT NULL";
        List<Member> members = (await connection.QueryAsync<Member>(memberSql)).ToList();

        Dictionary<string, List<Member>> membersByOrgGuid = members
            .GroupBy(m => m.OrganisationGuid)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (Organisation org in organisations)
            org.Members = membersByOrgGuid.TryGetValue(org.Guid, out var list) ? list : new List<Member>();

        stopwatch.Stop();
        Console.WriteLine($"GetAllOrganisationsAsync took {stopwatch.ElapsedMilliseconds} ms");

        return organisations;
    }

    public async Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationsAsync(string query, int limit = 10)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql = """
                           SELECT orgid AS Id, orgname AS Name
                               FROM kborgindex
                               WHERE LOCATE(@query, orgname) > 0
                               ORDER BY LOCATE(@query, orgname), orgname
                               LIMIT @limit
                           """;

        Stopwatch stopwatch = Stopwatch.StartNew();
        IEnumerable<OrganisationSearchResult> results =
            await connection.QueryAsync<OrganisationSearchResult>(sql, new { query, limit });
        stopwatch.Stop();
        Console.WriteLine($"SearchOrganisationsAsync took {stopwatch.ElapsedMilliseconds} ms for query: '{query}'");

        return results;
    }

    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        await using var connection = new MySqlConnection(ConnectionString);

        var extras = new (string SqlExpr, string Alias)[] {
            ("kborgindex.orgid",   "OrganisationId"),
            ("kborgindex.orgname", "OrganisationName")
        };

        string sql =
            BuildSelectSql(TableDefinitions.MemberTable, extras) +
            " LEFT JOIN kborgindex ON kborgindex.orgguid = members.orgguid" +
            " WHERE members.id = @id";

        return await connection.QueryFirstOrDefaultAsync<Member>(sql, new { id });
    }

    public async Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(string query, int limit = 10)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql = """
                           SELECT id AS Id, firstname AS FirstName, lastname AS LastName, email AS Email
                               FROM members
                               WHERE firstname LIKE @pattern OR lastname LIKE @pattern OR email LIKE @pattern
                               ORDER BY LOCATE(@query, firstname), LOCATE(@query, lastname), LOCATE(@query, email), firstname
                               LIMIT @limit
                           """;

        string pattern = $"%{query}%";

        return await connection.QueryAsync<MemberSearchResult>(sql, new { query, pattern, limit });
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