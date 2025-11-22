using AdminApi.Entities;
using Dapper;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    private async Task<List<string>> GetStringDatesFromDatabase(MySqlConnection connection, string sql, object param)
    {
        var result = await connection.QueryAsync<DateTime>(sql, param);
        return result.Select(d => d.ToString("yyyy-MM-dd")).ToList();
    }

    private async Task<MemberActivity> GetMemberActivityAsync(int memberId, string email)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string loginSql = """
                                SELECT DISTINCT DATE(logindate) AS Day
                                FROM loginhistory
                                WHERE memberid = @memberId
                                  AND logindate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string emailSql = """
                                SELECT DISTINCT DATE(sentdate) AS Day
                                FROM emailhistory
                                WHERE destemail = @email
                                  AND sent = 1
                                  AND sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string errorSql = """
                                SELECT DISTINCT DATE(sentdate) AS Day
                                FROM emailhistory
                                WHERE destemail = @email
                                  AND (sent = 0 OR NULLIF(TRIM(errortext), '') IS NOT NULL)
                                  AND sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string ninjaSql = """
                                SELECT DISTINCT DATE(c.createdate) AS Day
                                FROM members m
                                JOIN kborgindex o ON o.orgguid = m.orgguid
                                JOIN tagptcosts c ON c.orgid = o.orgid
                                WHERE m.id = @memberId
                                  AND c.createdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        List<string> loginDays = await GetStringDatesFromDatabase(connection, loginSql, new { memberId });
        List<string> emailDays = await GetStringDatesFromDatabase(connection, emailSql, new { email });
        List<string> emailErrorDays = await GetStringDatesFromDatabase(connection, errorSql, new { email });
        List<string> ninjaDays = await GetStringDatesFromDatabase(connection, ninjaSql, new { memberId });

        List<string> tenderNinjaDays = ninjaDays.Where(day => loginDays.Contains(day)).ToList();

        return new() {
            LoginDays = loginDays,
            EmailDays = emailDays,
            EmailErrorDays = emailErrorDays,
            TenderNinjaDays = tenderNinjaDays
        };
    }
}
