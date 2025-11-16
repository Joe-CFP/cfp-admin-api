using System.Diagnostics;
using AdminApi.Db;
using Dapper;
using MySql.Data.MySqlClient;
using AdminApi.Entities;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<List<OrganisationPreview>> GetAllOrganisationPreviewsAsync()
    {
        await using MySqlConnection connection = new(ConnectionString);

        Stopwatch stopwatch = Stopwatch.StartNew();

        const string sql = """
                           SELECT o.orgid AS Id, o.orgname AS Name, COUNT(m.id) AS MemberCount
                           FROM kborgindex o
                           LEFT JOIN members m ON m.orgguid = o.orgguid
                           GROUP BY o.orgid, o.orgname
                           ORDER BY o.orgname
                           """;

        List<OrganisationPreview> results = (await connection.QueryAsync<OrganisationPreview>(sql)).ToList();

        stopwatch.Stop();
        Console.WriteLine($"GetAllOrganisationPreviewsAsync took {stopwatch.ElapsedMilliseconds} ms");

        return results;
    }

    public async Task<Organisation?> GetOrganisationByIdAsync(int id)
    {
        await using MySqlConnection connection = new(ConnectionString);

        string orgSql = BuildSelectSql(TableDefinitions.OrganisationTable) +
                        " WHERE kborgindex.orgid = @id";
        OrganisationRecord? record =
            await connection.QueryFirstOrDefaultAsync<OrganisationRecord>(orgSql, new { id });
        if (record == null)
            return null;

        List<MemberPreview> members = await GetMemberPreviewsByOrgGuidAsync(record.Guid);

        Organisation org = record.ToOrganisation(members);
        org.MemberCount = members.Count;
        return org;
    }

    private async Task<List<MemberPreview>> GetMemberPreviewsByOrgGuidAsync(string orgGuid)
    {
        await using MySqlConnection connection = new(ConnectionString);
        string sql = GetMemberPreviewBaseSql() + " WHERE m.orgguid = @orgGuid ORDER BY m.username";
        return (await connection.QueryAsync<MemberPreview>(sql, new { orgGuid })).ToList();
    }
}
