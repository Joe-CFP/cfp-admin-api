using System.Data;
using System.Diagnostics;
using AdminApi.Db;
using Dapper;
using MySql.Data.MySqlClient;
using AdminApi.Entities;
using static AdminApi.Entities.MemberRecord;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<List<OrganisationSearchResult>> GetAllOrganisationSummariesAsync()
    {
        await using MySqlConnection connection = new(ConnectionString);

        Stopwatch stopwatch = Stopwatch.StartNew();

        const string sql = """
                           SELECT orgid AS Id, orgname AS Name
                           FROM kborgindex
                           ORDER BY orgname
                           """;

        List<OrganisationSearchResult> results = (await connection.QueryAsync<OrganisationSearchResult>(sql)).ToList();

        stopwatch.Stop();
        Console.WriteLine($"GetAllOrganisationSummariesAsync took {stopwatch.ElapsedMilliseconds} ms");

        return results;
    }

    public async Task<Organisation> GetOrganisationByIdAsync(int id)
    {
        await using MySqlConnection connection = new(ConnectionString);

        string orgSql = BuildSelectSql(TableDefinitions.OrganisationTable) +
                        " WHERE kborgindex.orgid = @id";
        OrganisationRecord? record =
            await connection.QueryFirstOrDefaultAsync<OrganisationRecord>(orgSql, new { id });
        if (record == null)
            throw new DataException("Couldn't retrieve Organisation record from database");

        List<MemberPreview> members = await GetMemberPreviewsByOrgGuidAsync(record.Guid);
        MemberActivity activity = await GetOrganisationMemberActivityAsync(record.Id);

        return record.ToOrganisation(members, activity);
    }

    private async Task<List<MemberPreview>> GetMemberPreviewsByOrgGuidAsync(string orgGuid)
    {
        await using MySqlConnection connection = new(ConnectionString);
        string sql = GetMemberPreviewBaseSql() + " WHERE m.orgguid = @orgGuid ORDER BY m.username";
        return (await connection.QueryAsync<MemberPreview>(sql, new { orgGuid })).ToList();
    }
}