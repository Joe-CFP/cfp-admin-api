using System.Data;
using AdminApi.Db;
using Dapper;
using MySql.Data.MySqlClient;
using AdminApi.Entities;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<List<OrganisationSearchResult>> GetAllOrganisationSummariesAsync()
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql = """
                           SELECT orgid AS Id, orgname AS Name
                           FROM kborgindex
                           ORDER BY orgname
                           """;

        return (await connection.QueryAsync<OrganisationSearchResult>(sql)).ToList();
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

        List<Member> members = await GetMembersByOrgGuidAsync(record.Guid);
        MemberActivity activity = await GetOrganisationMemberActivityAsync(record.Id);

        return record.ToOrganisation(members, activity);
    }

    private async Task<List<Member>> GetMembersByOrgGuidAsync(string orgGuid)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string idsSql = "SELECT id FROM members WHERE orgguid = @orgGuid";
        IEnumerable<int> memberIds = await connection.QueryAsync<int>(idsSql, new { orgGuid });

        List<Member> members = new();
        foreach (int memberId in memberIds)
        {
            Member m = await GetMemberByIdAsync(memberId);
            members.Add(m);
        }

        return members;
    }
}