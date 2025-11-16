using System.Data;
using AdminApi.Db;
using AdminApi.DTO;
using AdminApi.Entities;
using Dapper;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        await using MySqlConnection connection = new(ConnectionString);

        var extras = new (string SqlExpr, string Alias)[]
        {
            ("kborgindex.orgid", "OrganisationId"),
            ("kborgindex.orgname", "OrganisationName"),
            ("uj.curstate", "CurrentState"),
            ("(SELECT COUNT(*) FROM members m2 WHERE m2.orgguid = members.orgguid)", "OrganisationMemberCount")
        };

        string sql = BuildSelectSql(TableDefinitions.MemberTable, extras) +
                     " LEFT JOIN kborgindex ON kborgindex.orgguid = members.orgguid" +
                     " LEFT JOIN userjourney uj ON uj.username = members.email" +
                     " WHERE members.id = @id";

        MemberRecord? record = await connection.QueryFirstOrDefaultAsync<MemberRecord>(sql, new { id });
        if (record == null) return null;

        MemberOptions options = await GetMemberOptionsByMemberIdAsync(record.Id);
        DateTime? estimatedRegistrationDate = await GetEarliestInteractionAsync(record.Id);

        return record.ToMember(options, estimatedRegistrationDate);
    }

    public async Task<UserJourney?> GetMemberJourneyByIdAsync(int id)
    {
        await using MySqlConnection connection = new(ConnectionString);
        const string sql = "SELECT username FROM members WHERE id = @id";
        string? username = await connection.ExecuteScalarAsync<string?>(sql, new { id });
        if (string.IsNullOrWhiteSpace(username)) return null;
        return await GetUserJourneyByUsernameAsync(username);
    }

    public async Task<MemberActivity?> GetMemberActivityByIdAsync(int id)
    {
        await using MySqlConnection connection = new(ConnectionString);
        const string sql = "SELECT email FROM members WHERE id = @id";
        string? email = await connection.ExecuteScalarAsync<string?>(sql, new { id });
        if (string.IsNullOrWhiteSpace(email)) return null;
        MemberActivity activity = await GetMemberActivityAsync(id, email);
        return activity;
    }

    private async Task<UserJourney> GetUserJourneyByUsernameAsync(string username)
    {
        await using MySqlConnection connection = new(ConnectionString);

        string sql = $"{BuildSelectSql(TableDefinitions.UserJourneyTable)} WHERE username = @username";

        UserJourneyRecord? record = await connection.QueryFirstOrDefaultAsync<UserJourneyRecord>(sql, new { username });

        return record?.ToUserJourney()
            ?? new UserJourney {
                Username = username, CurrentState = "Missing User Journey",
                CurrentStateDateTime = DateTime.MinValue,
                Actions = new(), History = new(), Events = new()
            };
    }

    private async Task<MemberOptions> GetMemberOptionsByMemberIdAsync(int memberId)
    {
        await using MySqlConnection connection = new(ConnectionString);

        string sql = BuildSelectSql(TableDefinitions.MemberOptionsTable) + " WHERE id = @memberId";
        MemberOptionsRecord? record =
            await connection.QueryFirstOrDefaultAsync<MemberOptionsRecord>(sql, new { memberId });
        if (record == null) throw new DataException("Couldn't retrieve MemberOptions record from database");
        return record.ToMemberOptions();
    }

    private async Task<DateTime?> GetEarliestInteractionAsync(int memberId)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string memberSql = "SELECT email, mod_timestamp FROM members WHERE id = @id";
        var member =
            await connection.QueryFirstOrDefaultAsync<(string Email, DateTime? ModTimestamp)>(memberSql,
                new { id = memberId });
        if (member.Email == null)
            throw new DataException($"Couldn't retrieve member {memberId}");

        const string emailSql = "SELECT MIN(sentdate) FROM emailhistory WHERE destemail = @email";
        DateTime? earliestEmailDate =
            await connection.ExecuteScalarAsync<DateTime?>(emailSql, new { email = member.Email });

        DateTime? modDate = member.ModTimestamp == DateTime.MinValue ? null : member.ModTimestamp;
        if (modDate == null && earliestEmailDate == null) return null;
        if (modDate == null) return earliestEmailDate;
        if (earliestEmailDate == null) return modDate;

        return modDate < earliestEmailDate ? modDate : earliestEmailDate;
    }

    private static string GetMemberPreviewBaseSql()
    {
        return $"""
            SELECT 
                m.id AS Id,
                m.username AS Username,
                m.firstname AS FirstName,
                m.lastname AS LastName,
                uj.curstate AS CurrentState,
                {MemberRecord.SubscriptionCaseSql},
                kborgindex.orgid AS OrganisationId,
                kborgindex.orgname AS OrganisationName
                FROM members m
                    LEFT JOIN memberoptions mo ON mo.id = m.id
                    LEFT JOIN userjourney uj ON uj.username = m.email
                    LEFT JOIN kborgindex ON kborgindex.orgguid = m.orgguid
        """;
    }

    public async Task<IEnumerable<MemberPreview>> SearchMembersAsync(string query, int limit = 10)
    {
        await using MySqlConnection connection = new(ConnectionString);

        string sql = GetMemberPreviewBaseSql() + "\n" +
                     """
                     WHERE m.firstname LIKE @pattern OR m.lastname LIKE @pattern OR m.email LIKE @pattern
                     ORDER BY LOCATE(@query, m.firstname), LOCATE(@query, m.lastname), 
                              LOCATE(@query, m.email), m.firstname, m.lastname
                     LIMIT @limit
                     """;

        return await connection.QueryAsync<MemberPreview>(sql,
            new { query, pattern = $"%{query}%", limit = Math.Min(limit, 100) });
    }

    public async Task<MemberPreview?> GetMemberPreviewByEmailAsync(string email)
    {
        await using MySqlConnection connection = new(ConnectionString);
        string sql = GetMemberPreviewBaseSql() + " WHERE m.email = @Email LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<MemberPreview>(sql, new { Email = email });
    }
}
